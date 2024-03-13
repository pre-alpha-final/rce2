using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rce2;

namespace HostingAgent;

public class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<AppDataContext>();
        builder.Services.AddSingleton<Rce2Service>();
        builder.Services.AddTransient<WwwHandler>();
        builder.Services.AddTransient<TemplateHandler>();

        var host = builder.Build();

        host.Services.GetService<Rce2Service>()!.Run();

        await host.RunAsync();
    }
}
