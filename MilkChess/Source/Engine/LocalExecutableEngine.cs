using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MilkChess.Engine;

public class LocalExecutableEngine : INativeUcciEngine
{
    private readonly string ExecutableFile;
    private readonly Collection<string> Arguments;

    private Process Process;

    public LocalExecutableEngine(string executableFile, Collection<string> arguments)
    {
        this.ExecutableFile = executableFile;
        this.Arguments = arguments;
    }

    public void Initialize()
    {
        var startInfo = new ProcessStartInfo()
        {
                        FileName = this.ExecutableFile,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
        };

        if (Arguments?.Count > 0)
        {
            foreach (var argument in this.Arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }
        }


        this.Process = Process.Start(startInfo);
    }

    public void SendCommand(string command)
    {
        this.Process.StandardInput.WriteLine(command);
    }

    public string GetFeedback()
    {
        return this.Process.StandardOutput.ReadLine();
    }

    public IEnumerable<string> DiscardBufferData()
    {
        while (this.Process.StandardOutput.Peek() > -1)
        {
            yield return this.GetFeedback();
        }
    }

    public void Dispose()
    {
        this.Process?.Dispose();
    }
}