using Broker.Server.Services;
using Broker.Shared.Events;
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

        public BrokerController(IFeedService<BrokerEventBase> feedService)
        {
            _feedService = feedService;
        }

        [HttpGet("{id:Guid}")]
        public async Task<OkObjectResult> GetFeed(Guid id)
        {
            return Ok((await _feedService.GetNext(id)).Select(e => Convert.ChangeType(e, e.GetType())));
        }
    }
}
