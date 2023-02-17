using Broker.Server.Services;
using Broker.Server.Services.Implementation;
using Broker.Shared.Events;
using Broker.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Server.Controllers
{
    [ApiController]
    [Route("api/broker")]
    [Authorize]
    public class BrokerController : ControllerBase
    {
        private readonly IFeedService<BrokerEventBase> _feedService;
        private readonly IBindingRepository _bindingRepository;

        public BrokerController(IFeedService<BrokerEventBase> feedService, IBindingRepository bindingRepository)
        {
            _feedService = feedService;
            _bindingRepository = bindingRepository;
        }

        [HttpGet("{id:Guid}")]
        public async Task<OkObjectResult> GetFeed(Guid id)
        {
            return Ok((await _feedService.GetNext(id)).Select(e => Convert.ChangeType(e, e.GetType())));
        }

        [HttpPost("createBinding")]
        public async Task<OkResult> CreateBinding([FromBody] Binding binding)
        {
            if (_bindingRepository.AddBinding(binding))
            {
                await _feedService.BroadcastItem(new BindingAddedEvent
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
                await _feedService.BroadcastItem(new BindingDeletedEvent
                {
                    Binding = binding,
                });
            }

            return Ok();
        }
    }
}
