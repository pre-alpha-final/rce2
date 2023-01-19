using Broker.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Server.Controllers
{
    [ApiController]
    [Route("api/broker")]
    public class BrokerController : ControllerBase
    {
        private readonly IFeedService _feedService;

        public BrokerController(IFeedService feedService)
        {
            _feedService = feedService;
        }

        [HttpGet("{id:Guid}")]
        public async Task<OkObjectResult> GetFeed(Guid id)
        {
            return Ok(await _feedService.GetNext(id));
        }
    }
}
