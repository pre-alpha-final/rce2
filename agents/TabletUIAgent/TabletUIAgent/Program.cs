using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Rce2;

namespace TabletUIAgent;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddHttpClient();
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton<Rce2Service>();

        var host = builder.Build();
        SetUpRce2(host.Services.GetService<Rce2Service>()!);

        await host.RunAsync();
    }

    private static void SetUpRce2(Rce2Service rce2Service)
    {
        rce2Service
            .SetBrokerAddress("https://localhost:7113")
            .SetAgentId(Guid.NewGuid())
            .SetAgentKey(string.Empty)
            .SetAgentName("TabletUI")
            .SetInputDefinitions(new()
            {
                { "sendit-input", Rce2Types.String }
            })
            .SetOutputDefinitions(new()
            {
                { "yt-back-10", Rce2Types.Number },
                { "yt-back", Rce2Types.Number },
                { "yt-pause-resume", Rce2Types.Void },
                { "yt-forward", Rce2Types.Number },
                { "yt-forward-10", Rce2Types.Number },
                { "sendit-output", Rce2Types.String }
            })
            .Init();
    }
}
