using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkChess.Helper;

namespace MilkChess.Engine;

public class LocalExecutableEngine : INativeUciEngine
{
    private readonly UciLogger UciLogger;
    private readonly ILogger<LocalExecutableEngine> Logger;
    private readonly EngineConfig Config;

    public string Name { get; private set; }

    private Process Process;

    public LocalExecutableEngine(
        ILogger<LocalExecutableEngine> logger,
        MilkChessConfig config,
        UciLogger UciLogger)
    {
        this.Logger = logger;
        this.UciLogger = UciLogger;
        this.Config = config.DefaultEngineConfig.Clone();

        this.Name = this.Config.Name;
    }

    public void Initialize()
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = this.Config.Path,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
        };

        if (this.Config.Arguments?.Count > 0)
        {
            foreach (var argument in this.Config.Arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }
        }


        this.Process = Process.Start(startInfo);
    }

    public async Task SendCommandAsync(string command)
    {
        this.UciLogger.WriteCommand(command);
        await this.Process.StandardInput.WriteLineAsync(command).ConfigureAwait(false);
    }

    public async Task<UciFeedback> GetFeedbackAsync(CancellationToken cancellationToken)
    {
        string feedback = await this.Process.StandardOutput.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        this.UciLogger.WriteFeedback(feedback);

        if (!UciFeedback.TryParse(feedback, out UciFeedback uciFeedback))
        {
            Logger.LogError("Can't parse feedback: {Feedback}", feedback);
            return null;
        }
        else
        {
            return uciFeedback;
        }
    }


    public void Dispose()
    {
        this.Process?.Dispose();
        this.UciLogger.Dispose();
    }
}