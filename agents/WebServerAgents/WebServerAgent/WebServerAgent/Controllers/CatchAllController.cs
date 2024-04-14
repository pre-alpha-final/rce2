using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PubSub;
using Rce2;
using System.Text;
using System.Text.RegularExpressions;

namespace WebServerAgent.Controllers;

[ApiController]
public class CatchAllController : ControllerBase, IDisposable
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Rce2Service _rce2Service;
    private string? _rce2RequestId = null;
    private Rce2Message? _rce2Response = null;

    public CatchAllController(IHttpContextAccessor httpContextAccessor, Rce2Service rce2Service)
    {
        _httpContextAccessor = httpContextAccessor;
        _rce2Service = rce2Service;
    }

    public void Dispose()
    {
        Hub.Default.Unsubscribe(this);
    }

    [HttpGet]
    [HttpPost]
    [Route("/{**slug}")]
    public async Task CatchAll()
    {
        var path = _httpContextAccessor.HttpContext!.Request.Path.ToString();
        var agentId = GetRouteAgentId(path);
        var pathRemnant = GetPathRemnant(path);
        if (agentId != null)
        {
            SetCookie("rce2_agent_id", agentId);
            _httpContextAccessor.HttpContext!.Response.Redirect(pathRemnant == string.Empty ? "/" : pathRemnant);
            return;
        }

        agentId = GetCookieAgentId(_httpContextAccessor.HttpContext!.Request.Cookies);
        if (agentId == null)
        {
            await SetErrorResponse("Agent association missing");
            return;
        }

        var rce2Sync = new Rce2Sync<List<string>>(async (e, _) => e.GetValue("rce2_request_id") == _rce2RequestId);
        await SendRce2Message(agentId);
        await rce2Sync.WaitAsync();
        _rce2Response = rce2Sync.Result;

        if (_rce2Response == null)
        {
            await SetErrorResponse("Agent timeout");
            return;
        }

        await SetResponse();
    }

    private string? GetRouteAgentId(string path)
    {
        var regex = new Regex(@"^\/([^\/]{36})");
        var match = regex.Match(path);

        return match.Groups.Count == 2
            ? match.Groups[1].Value
            : null;
    }

    private string GetPathRemnant(string path)
    {
        return path.Substring(path.Length >= 37 ? 37 : 0);
    }

    private void SetCookie(string key, string value)
    {
        _httpContextAccessor.HttpContext!.Response.Cookies.Append(key, value, new()
        {
            Path = "/"
        });
    }

    private string? GetCookieAgentId(IRequestCookieCollection cookies)
    {
        return cookies.FirstOrDefault(e => e.Key == "rce2_agent_id").Value;
    }

    private async Task SetErrorResponse(string errorResponse)
    {
        _httpContextAccessor.HttpContext!.Response.Headers["content-type"] = "text/html";
        var errorBody = Encoding.UTF8.GetBytes(errorResponse);
        await _httpContextAccessor.HttpContext.Response.Body.WriteAsync(errorBody, 0, errorBody.Length);
    }

    private async Task SendRce2Message(string agentId)
    {
        using var bodyStreamReader = new StreamReader(_httpContextAccessor.HttpContext!.Request.Body);

        _rce2RequestId = Guid.NewGuid().ToString();
        var requestPayload = new List<string>
        {
            "rce2_request_id",
            _rce2RequestId,
            "rce2_agent_id",
            agentId,
            "method",
            _httpContextAccessor.HttpContext.Request.Method,
            "path",
            _httpContextAccessor.HttpContext.Request.Path.ToString(),
            "query_string",
            _httpContextAccessor.HttpContext.Request.QueryString.ToString(),
            "body",
            await bodyStreamReader.ReadToEndAsync(),
        };
        await _rce2Service.HandleRequest(requestPayload);
    }

    private async Task SetResponse()
    {
        var payloadData = _rce2Response!.Payload?["data"]?.ToObject<List<string>>();
        var body = GZipHelpers.GUnZip(JsonConvert.DeserializeObject<byte[]>(payloadData!.GetValue("body"))!);
        if (body == null)
        {
            await SetErrorResponse("Malformed body");
            return;
        }

        _httpContextAccessor.HttpContext!.Response.Headers["content-type"] = payloadData.GetValue("content_type");
        await _httpContextAccessor.HttpContext.Response.Body.WriteAsync(body, 0, body.Length);
    }
}
