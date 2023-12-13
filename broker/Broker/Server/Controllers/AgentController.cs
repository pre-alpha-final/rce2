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

    public AgentController(IAgentFeedService agentFeedService, IBrokerFeedService brokerFeedService,
        IBindingRepository bindingRepository)
    {
        _agentFeedService = agentFeedService;
        _brokerFeedService = brokerFeedService;
        _bindingRepository = bindingRepository;
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

        await _brokerFeedService.BroadcastItem(new AgentOutputReceivedEvent
        {
            AgentId = id,
            Contact = rce2Message.Contact,
            Payload = rce2Message.Payload,
        });

        var bindings = _bindingRepository.GetBindingsFrom(id, rce2Message.Contact);
        foreach(var binding in bindings)
        {
            var rce2MessageClone = rce2Message.Clone();
            rce2MessageClone.Contact = binding.InContact;
            await _agentFeedService.AddItem(binding.InId, rce2MessageClone);

            await _brokerFeedService.BroadcastItem(new AgentInputReceivedEvent
            {
                AgentId = binding.InId,
                Contact = rce2MessageClone.Contact,
                Payload = rce2MessageClone.Payload,
            });
        }

        return Ok();
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
