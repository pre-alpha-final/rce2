using Microsoft.Extensions.Hosting;
using PubSub;
using Rce2;

namespace MudslideAgent;

public class App : IHostedService
{
    private readonly Rce2Service _rce2Service;
    private readonly MudslideService _mudslideService;

    public App(Rce2Service rce2Service, MudslideService mudslideService)
    {
        _rce2Service = rce2Service;
        _mudslideService = mudslideService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _rce2Service
            .SetBrokerAddress("https://localhost:7113")
            .SetAgentId(Guid.NewGuid())
            .SetAgentKey(string.Empty)
            .SetAgentName("Mudslide")
            .SetInputDefinitions(new()
            {
                { "send", Rce2Types.String }
            })
            .Init();

        var output = await _mudslideService.Send("Connectivity test");
        Console.WriteLine(Environment.NewLine + output);

        Hub.Default.Subscribe<Rce2Message>(this, async e =>
        {
            if (e.Contact != "send")
            {
                return;
            }

            var output = await _mudslideService.Send(e.Payload["data"]?.ToObject<string>());
            Console.WriteLine(Environment.NewLine + output);
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Hub.Default.Unsubscribe(this);
    }
}
