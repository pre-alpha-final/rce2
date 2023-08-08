using Broker.Server.Services;
using Broker.Shared.Events;
using Broker.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Server.Controllers;

[ApiController]
[Route("api/broker")]
[Authorize]
public class BrokerController : ControllerBase
{
    private readonly IFeedService<BrokerEventBase> _brokerFeedService;
    private readonly IFeedService<Rce2Message> _agentFeedService;
    private readonly IBindingRepository _bindingRepository;
    private readonly IEchoFeedRepository<BrokerEventBase> _echoFeedRepository;

    public BrokerController(IFeedService<BrokerEventBase> brokerFeedService, IFeedService<Rce2Message> agentFeedService,
        IBindingRepository bindingRepository, IEchoFeedRepository<BrokerEventBase> echoFeedRepository)
    {
        _brokerFeedService = brokerFeedService;
        _agentFeedService = agentFeedService;
        _bindingRepository = bindingRepository;
        _echoFeedRepository = echoFeedRepository;
    }

    [HttpGet("{id:Guid}")]
    public async Task<OkObjectResult> GetFeed(Guid id)
    {
        if (_brokerFeedService.Exists(id) == false)
        {
            await _brokerFeedService.AddItems(id, _echoFeedRepository.GetAll());
            await _brokerFeedService.AddItems(id, _bindingRepository.GetAll().Select(e => new BindingAddedEvent
            {
                Binding = e,
            }));
            await _agentFeedService.BroadcastItem(new Rce2Message
            {
                Type = Rce2Types.WhoIs,
            });
        }

        return Ok((await _brokerFeedService.GetNext(id)).Select(e => Convert.ChangeType(e, e.GetType())));
    }

    [HttpPost("createBinding")]
    public async Task<OkResult> CreateBinding([FromBody] Binding binding)
    {
        if (_bindingRepository.AddBinding(binding))
        {
            await _brokerFeedService.BroadcastItem(new BindingAddedEvent
            {
                Binding = binding,
            });
        }

        return Ok();
    }

    [HttpPost("deleteBinding")]
    public async Task<OkResult> DeleteBinding([FromBody] Binding binding)
    {
        if (_bindingRepository.DeleteBinding(binding))
        {
            await _brokerFeedService.BroadcastItem(new BindingDeletedEvent
            {
                Binding = binding,
            });
        }

        return Ok();
    }

    [HttpPost("simulateIn/{id:Guid}")]
    public async Task<OkResult> SimulateIn(Guid id, [FromBody] Rce2Message rce2Message)
    {
        await _brokerFeedService.BroadcastItem(new AgentSimulatedInputEvent
        {
            AgentId = id,
            Contact = rce2Message.Contact,
            Payload = rce2Message.Payload,
        });

        await _agentFeedService.AddItem(id, rce2Message);

        return Ok();
    }

    [HttpPost("simulateOut/{id:Guid}")]
    public async Task<OkResult> SimulateOut(Guid id, [FromBody] Rce2Message rce2Message)
    {
        await _brokerFeedService.BroadcastItem(new AgentSimulatedOutputEvent
        {
            AgentId = id,
            Contact = rce2Message.Contact,
            Payload = rce2Message.Payload,
        });

        var bindings = _bindingRepository.GetBindingsFrom(id, rce2Message.Contact);
        foreach (var binding in bindings)
        {
            rce2Message.Contact = binding.InContact;
            await _agentFeedService.AddItem(binding.InId, rce2Message);
        }

        return Ok();
    }
}
