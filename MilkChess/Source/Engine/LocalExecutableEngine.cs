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
    private readonly MilkChessConfig Config;

    private Process Process;

    public LocalExecutableEngine(IOptions<MilkChessConfig> config, ILogger<LocalExecutableEngine> logger,
        UciLogger UciLogger)
    {
        this.Logger = logger;
        this.UciLogger = UciLogger;
        this.Config = config.Value;
    }

    public void Initialize()
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = this.Config.Engine.Path,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
        };

        if (this.Config.Engine.Arguments?.Count > 0)
        {
            foreach (var argument in this.Config.Engine.Arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }
        }


        this.Process = Process.Start(startInfo);
    }

    public void SendCommand(string command)
    {
        this.UciLogger.WriteCommand(command);
        this.Process.StandardInput.WriteLine(command);
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

    public UciFeedback GetFeedback()
    {
        string feedback = this.Process.StandardOutput.ReadLine();
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