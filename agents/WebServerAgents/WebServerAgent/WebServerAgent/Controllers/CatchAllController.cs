using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace WebServerAgent.Controllers
{
    [ApiController]
    public class CatchAllController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CatchAllController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        [HttpPost]
        [Route("/{**slug}")]
        public async Task CatchAll()
        {
        }

        private async Task NoMatch()
        {
            var content = Encoding.UTF8.GetBytes("<html><body>No matching url</body></html>");
            _httpContextAccessor.HttpContext.Response.Headers["content-type"] = "text/html; charset=utf-8";
            await _httpContextAccessor.HttpContext.Response.Body.WriteAsync(content, 0, content.Length);
        }
    }
}
