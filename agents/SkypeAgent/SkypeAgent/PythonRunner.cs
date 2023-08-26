using System.Diagnostics;

namespace SkypeAgent;

public class PythonRunner
{
    public async Task<string> Run(string script, string arguments)
    {
        var scriptFile = $"{AppContext.BaseDirectory}{Guid.NewGuid()}.py";

        try
        {
            File.WriteAllText(scriptFile, script);

            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "python",
                Arguments = $" {scriptFile} {arguments}",
            };

            Process process = Process.Start(processStartInfo);
            process.WaitForExit();
            
            return process.StandardOutput.ReadToEnd();
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            File.Delete(scriptFile);
        }
    }
}
