using Newtonsoft.Json;

namespace Rce2;

public class WwwHandler
{
    private readonly TemplateHandler _templateHandler;

    public WwwHandler(TemplateHandler templateHandler)
    {
        _templateHandler = templateHandler;
    }

    public async Task<List<string>> Handle(List<string> args)
    {
        var renderResponse = new List<string>();
        renderResponse.AddRange(["rce2_request_id", args.GetValue("rce2_request_id")]);
        renderResponse.AddRange(["content_type", GetContentType(args.GetValue("path"))]);
        renderResponse.AddRange(["body", await GetBody(args)]);

        return renderResponse;
    }

    private string GetContentType(string path)
    {
        return path switch
        {
            string e when e.EndsWith(".css") => "text/css; charset=utf-8",
            string e when e.EndsWith(".js") => "application/javascript; charset=utf-8",
            string e when e.EndsWith(".json") => "application/x-www-form-urlencoded;charset=utf-8",
            string e when e.EndsWith(".ico") => "image/x-icon",
            string e when e.EndsWith(".svg") => "image/svg+xml",
            string e when e.EndsWith(".png") => "image/png",
            _ => "text/html; charset=utf-8"
        };
    }

    private async Task<string> GetBody(List<string> args)
    {
        byte[] bodyBytes;

        if (args.GetValue("path").Contains(".."))
        {
            throw new ArgumentException("Invalid path");
        }

        if (args.GetValue("path") == "/")
        {
            bodyBytes = await File.ReadAllBytesAsync($"{AppDomain.CurrentDomain.BaseDirectory}/www/index.html");
            return _templateHandler.HandleIndex(bodyBytes);
        }

        bodyBytes = await File.ReadAllBytesAsync($"{AppDomain.CurrentDomain.BaseDirectory}/www/{args.GetValue("path")}");
        return JsonConvert.SerializeObject(GZipHelpers.GZip(bodyBytes));
    }
}
