using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkChess.Helper;

namespace MilkChess.Engine;

public class LocalExecutableEngine : INativeUcciEngine
{
    private readonly UcciLogger UcciLogger;
    private readonly ILogger<LocalExecutableEngine> Logger;
    private readonly MilkChessConfig Config;

    private Process Process;

    public LocalExecutableEngine(IOptions<MilkChessConfig> config, ILogger<LocalExecutableEngine> logger,
        UcciLogger ucciLogger)
    {
        this.Logger = logger;
        this.UcciLogger = ucciLogger;
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
        this.UcciLogger.WriteCommand(command);
        this.Process.StandardInput.WriteLine(command);
    }

    public async Task<UcciFeedback> GetFeedbackAsync(CancellationToken cancellationToken)
    {
        string feedback = await this.Process.StandardOutput.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        this.UcciLogger.WriteFeedback(feedback);

        if (!UcciFeedback.TryParse(feedback, out UcciFeedback ucciFeedback))
        {
            Logger.LogError($"Can't parse feedback: {feedback}");
            return null;
        }
        else
        {
            return ucciFeedback;
        }
    }

    public UcciFeedback GetFeedback()
    {
        string feedback = this.Process.StandardOutput.ReadLine();
        this.UcciLogger.WriteFeedback(feedback);

        if (!UcciFeedback.TryParse(feedback, out UcciFeedback ucciFeedback))
        {
            Logger.LogError($"Can't parse feedback: {feedback}");
            return null;
        }
        else
        {
            return ucciFeedback;
        }
    }


    public void Dispose()
    {
        this.Process?.Dispose();
        this.UcciLogger.Dispose();
    }
}