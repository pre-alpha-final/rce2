using Microsoft.Extensions.Hosting;
using PubSub;
using System.Collections.Concurrent;

namespace ChromeLikeWaterfallAgent;

public class App : IHostedService
{
    private ConcurrentBag<NameStartStop> nameStartStops = new();

    public App()
    {
        Hub.Default.Subscribe<NameStartStop>(this, OnNewItem);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested == false)
        {
            var offset = nameStartStops.OrderBy(e => e.Start).FirstOrDefault()?.Start;

            Console.Clear();
            foreach (var nameStartStop in nameStartStops.OrderByDescending(e => e.Start).ToList())
            {
                Console.WriteLine($"\"{nameStartStop.Name}\"\t{nameStartStop.Start - offset}\t{nameStartStop.Stop - nameStartStop.Start}");
            }
            await Task.Delay(1000);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Hub.Default.Unsubscribe(this);
    }

    private void OnNewItem(NameStartStop nameStartStop)
    {
        nameStartStops.Add(nameStartStop);
    }
}
