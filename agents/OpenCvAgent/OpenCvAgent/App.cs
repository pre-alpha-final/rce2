using Microsoft.Extensions.Hosting;
using OpenCvAgent.Infrastructure;
using Rce2;

namespace OpenCvAgent;

public class App : IHostedService
{
    private readonly Rce2Service _rce2Service;
    private readonly OpenCvService _openCvService;

    public App(Rce2Service rce2Service, OpenCvService openCvService)
    {
        _rce2Service = rce2Service;
        _openCvService = openCvService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _rce2Service
            .SetBrokerAddress("https://localhost:7113")
            .SetAgentId(Guid.NewGuid())
            .SetAgentName("OpenCv")
            //.SetChannels(new() { "" })
            .SetInputDefinitions(new()
            {
            })
            .SetOutputDefinitions(new()
            {
                { "match-found", Rce2Types.Number }
            })
            .Init();

        _openCvService.Init();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }
}
