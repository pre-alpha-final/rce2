using HostingAgent;
using Newtonsoft.Json;
using System.Text;

namespace Rce2;

public class TemplateHandler
{
    private readonly AppDataContext _appContext;

    public TemplateHandler(AppDataContext appContext)
    {
        _appContext = appContext;
    }

    public string HandleIndex(byte[] bodyBytes)
    {
        var content = Encoding.UTF8.GetString(bodyBytes);
        var newContent = content.Replace("RCE2_TEMPLATE_APP_TITLE", _appContext.AppTitle);

        return JsonConvert.SerializeObject(GZipHelpers.GZip(Encoding.UTF8.GetBytes(newContent)));
    }
}
