using System.Diagnostics;

namespace MudslideAgent;

public class MudslideService
{
    public async Task<string> Send(string message)
    {
        try
        {
            var filename = $"{Guid.NewGuid()}.txt";
            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "cmd.exe",
                Arguments = $" /C npx mudslide send me \"{message}\" 2>&1 > {filename}",
            };
            var process = Process.Start(processStartInfo);

            await process.WaitForExitAsync();
            var output = File.ReadAllText(filename);
            File.Delete(filename);

            return "Mudslide output: " + Environment.NewLine + output;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
