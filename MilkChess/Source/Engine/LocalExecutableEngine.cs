using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MilkChess.Engine;

public class LocalExecutableEngine : INativeUcciEngine
{
    private readonly ILogger Logger;
    private readonly string ExecutableFile;
    private readonly Collection<string> Arguments;

    private Process Process;

    public LocalExecutableEngine(ILogger logger, string executableFile, Collection<string> arguments)
    {
        this.Logger = logger;
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
        Logger.LogInformation($"[Send] {command}");
        this.Process.StandardInput.WriteLine(command);
    }

    public string GetFeedback()
    {
        string feedback = this.Process.StandardOutput.ReadLine();
        Logger.LogInformation($"[Received] {feedback}");
        return feedback;
    }

    public void DiscardBufferData()
    {
        while (this.Process.StandardOutput.Peek() > -1)
        {
            _ = this.GetFeedback();
        }
    }

    public void Dispose()
    {
        this.Process?.Dispose();
    }
}