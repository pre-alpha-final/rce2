using System.Diagnostics;

namespace MudslideAgent;

public class MudslideService
{
    public async Task Send(string message)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "cmd.exe",
                Arguments = $" /C npx mudslide send me \"{message}\"",
            };
            var process = Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
