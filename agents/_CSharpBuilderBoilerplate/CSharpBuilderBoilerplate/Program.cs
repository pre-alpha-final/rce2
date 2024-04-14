using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rce2;

namespace CSharpBuilderBoilerplate;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHostedService<App>();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<Rce2Service>();

        var host = builder.Build();

        host.Services.GetService<Rce2Service>()!
            .SetBrokerAddress("https://localhost:7113")
            .SetAgentId(Guid.NewGuid())
            .SetAgentName("Boilerplate")
            .SetChannels(new() { "boilerplate" })
            .SetInputDefinitions(new()
            {
                { "echo-test", Rce2Types.String }
            })
            .SetOutputDefinitions(new()
            {
                { "echo-test", Rce2Types.String }
            })
            .Run();

        await host.RunAsync();
    }
}
