using Broker.Server.Infrastructure;
using Broker.Server.Services;
using Broker.Shared.Events;
using Broker.Shared.Model;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Server.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly IAgentFeedService _agentFeedService;
    private readonly IBrokerFeedService _brokerFeedService;
    private readonly IBindingRepository _bindingRepository;
    private readonly IActiveAgentCache _activeAgentCache;

    public AgentController(IAgentFeedService agentFeedService, IBrokerFeedService brokerFeedService,
        IBindingRepository bindingRepository, IActiveAgentCache activeAgentCache)
    {
        _agentFeedService = agentFeedService;
        _brokerFeedService = brokerFeedService;
        _bindingRepository = bindingRepository;
        _activeAgentCache = activeAgentCache;
    }

    [HttpGet("{id:Guid}")]
    public async Task<OkObjectResult> GetFeed(Guid id)
    {
        if (_agentFeedService.Exists(id) == false)
        {
            await _agentFeedService.AddItem(id, new Rce2Message
            {
                Type = Rce2Types.WhoIs,
            });
        }

        return Ok(await _agentFeedService.GetNext(id));
    }

    [HttpPost("{id:Guid}")]
    public async Task<StatusCodeResult> PostOutput(Guid id, [FromBody] Rce2Message rce2Message)
    {
        if (_agentFeedService.Exists(id) == false)
        {
            await _agentFeedService.AddItem(id, new Rce2Message
            {
                Type = Rce2Types.WhoIs,
            });
        }

        if (rce2Message.Type == Rce2Types.WhoIs)
        {
            await PubSub.Hub.Default.PublishAsync(new WhoIsReceived
            {
                Agent = rce2Message.Payload.ToObject<Agent>(),
            });
            await _brokerFeedService.BroadcastItem(new AgentUpdatedEvent
            {
                Agent = rce2Message.Payload.ToObject<Agent>(),
            });
            await RecheckBindings();

            return Ok();
        }

        if (rce2Message.Contact == null)
        {
            return BadRequest();
        }

        await _brokerFeedService.BroadcastItem(new AgentOutputReceivedEvent(rce2Message.Payload)
        {
            AgentId = id,
            Contact = rce2Message.Contact,
        });

        var bindings = _bindingRepository.GetBindingsFrom(id, rce2Message.Contact);
        foreach(var binding in bindings)
        {
            await SendMessage(rce2Message, binding.InContact, binding.InId);
        }

        var matchingChannelAgents = _activeAgentCache.GetMatchingChannelAgents(id, rce2Message.Contact);
        foreach(var matchingChannelAgent in matchingChannelAgents)
        {
            await SendMessage(rce2Message, rce2Message.Contact, matchingChannelAgent.Id);
        }

        return Ok();
    }

    private async Task SendMessage(Rce2Message originalRce2Message, string inContact, Guid inId)
    {
        var rce2MessageClone = originalRce2Message.Clone();
        rce2MessageClone.Contact = inContact;
        await _agentFeedService.AddItem(inId, rce2MessageClone);

        await _brokerFeedService.BroadcastItem(new AgentInputReceivedEvent(rce2MessageClone.Payload)
        {
            AgentId = inId,
            Contact = rce2MessageClone.Contact,
        });
    }

    private async Task RecheckBindings()
    {
        var bindings = _bindingRepository.GetAll();
        foreach (var binding in bindings)
        {
            if (_agentFeedService.Exists(binding.OutId) && _agentFeedService.Exists(binding.InId))
            {
                if (binding.IsActive == false)
                {
                    binding.IsActive = true;
                    _bindingRepository.UpdateBinding(binding);
                    await _brokerFeedService.BroadcastItem(new BindingUpdatedEvent
                    {
                        Binding = binding,
                    });
                }
            }
            else
            {
                if (binding.IsActive)
                {
                    binding.IsActive = false;
                    _bindingRepository.UpdateBinding(binding);
                    await _brokerFeedService.BroadcastItem(new BindingUpdatedEvent
                    {
                        Binding = binding,
                    });
                }
            }
        }
    }
}
