using Microsoft.Extensions.Hosting;
using PubSub;
using Rce2;

namespace CSharpBuilderBoilerplate;

public class App : IHostedService
{
    private readonly Rce2Service _rce2Service;

    public App(Rce2Service rce2Service)
    {
        _rce2Service = rce2Service;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _rce2Service
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
            .Init();

        Hub.Default.Subscribe<Rce2Message>(this, async e =>
        {
            if (e.Contact != "echo-test")
            {
                return;
            }

            var payload = e.Payload["data"]?.ToObject<string>();
            await Task.Delay(1000);
            await _rce2Service.Send("echo-test", payload!);
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Hub.Default.Unsubscribe(this);
    }
}
