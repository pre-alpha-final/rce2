using Broker.Server.Services;
using Broker.Shared.Events;
using Broker.Shared.Model;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Server.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly IFeedService<Rce2Message> _agentFeedService;
    private readonly IFeedService<BrokerEventBase> _brokerFeedService;
    private readonly IBindingRepository _bindingRepository;

    public AgentController(IFeedService<Rce2Message> agentFeedService, IFeedService<BrokerEventBase> brokerFeedService,
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
        if (rce2Message.Type == Rce2Types.WhoIs)
        {
            await _brokerFeedService.BroadcastItem(new AgentUpdatedEvent
            {
                Agent = rce2Message.Payload.ToObject<Agent>(),
            });

            return Ok();
        }

        if (rce2Message.Contact == null)
        {
            return BadRequest();
        }

        await _brokerFeedService.BroadcastItem(new AgentOutputEvent
        {
            AgentId = id,
            Contact = rce2Message.Contact,
            Payload = rce2Message.Payload,
        });

        var bindings = _bindingRepository.GetBindingsFrom(id, rce2Message.Contact);
        foreach(var binding in bindings)
        {
            rce2Message.Contact = binding.InContact;
            await _agentFeedService.AddItem(binding.InId, rce2Message.Clone());
        }

        return Ok();
    }
}
