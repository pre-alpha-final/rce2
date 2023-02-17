using Broker.Server.Services;
using Broker.Shared.Model;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Server.Controllers
{
    [ApiController]
    [Route("api/agent")]
    public class AgentController : ControllerBase
    {
        private readonly IFeedService<Rce2Message> _feedService;
        private readonly IBindingRepository _bindingRepository;

        public AgentController(IFeedService<Rce2Message> feedService, IBindingRepository bindingRepository)
        {
            _feedService = feedService;
            _bindingRepository = bindingRepository;
        }

        [HttpGet("{id:Guid}")]
        public async Task<OkObjectResult> GetFeed(Guid id)
        {
            return Ok((await _feedService.GetNext(id)));
        }

        [HttpPost("{id:Guid}")]
        public async Task<OkResult> PostOutput(Guid id, [FromBody] Rce2Message rce2Message)
        {
            var bindings = _bindingRepository.GetBindingsFrom(id);
            foreach(var binding in bindings)
            {
                await _feedService.AddItem(binding.InId, rce2Message);
            }

            return Ok();
        }
    }
}
