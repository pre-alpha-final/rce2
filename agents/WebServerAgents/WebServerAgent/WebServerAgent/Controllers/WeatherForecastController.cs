using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebServerAgent.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ICompositeViewEngine viewEngine,
            ITempDataProvider tempDataProvider)
        {
            _logger = logger;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ViewEngineResult viewResult = _viewEngine.FindView(ControllerContext, "Component1", false);

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    new ViewDataDictionary(
                        new EmptyModelMetadataProvider(),
                        new ModelStateDictionary())
                    {
                        Model = null
                    },
                    new TempDataDictionary(HttpContext, _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);

                return Ok(output.ToString());
            }
        }
    }
}
