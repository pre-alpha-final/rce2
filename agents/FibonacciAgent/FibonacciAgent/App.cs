using Microsoft.Extensions.Hosting;
using PubSub;
using Rce2;

namespace FibonacciAgent;

public class App : IHostedService
{
    private int _previousNumber;
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
            .SetAgentKey(string.Empty)
            .SetAgentName("Fibonacci")
            .SetChannels(new() { "fibonacci" })
            .SetInputDefinitions(new()
            {
                { "fibonacci-data", Rce2Types.Number }
            })
            .SetOutputDefinitions(new()
            {
                { "fibonacci-data", Rce2Types.Number }
            })
            .Init();

        Hub.Default.Subscribe<Rce2Message>(this, async e =>
        {
            if (e.Contact == "fibonacci-data")
            {
                await Task.Delay(1000);

                var payload = e.Payload["data"]!.ToObject<int>();
                var nextNumber = _previousNumber + payload;
                _previousNumber = payload;

                await _rce2Service.Send("fibonacci-data", nextNumber);
            }
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Hub.Default.Unsubscribe(this);
    }
}
