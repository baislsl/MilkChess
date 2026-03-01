using Microsoft.Extensions.Logging;
using MilkChess.Model;

namespace MilkChess.Engine;

public class UciLogger
{
    private readonly ILogger<UciLogger> logger;
    private readonly object obj = new();

    private readonly FileStream fileStream;

    private readonly StreamWriter writer;

    public UciLogger(MilkChessConfig config, ILogger<UciLogger> logger)
    {
        this.logger = logger;
        string logFile = config.DefaultEngineConfig.Log;

        if (!string.IsNullOrWhiteSpace(logFile))
        {
            try
            {
                this.fileStream = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                this.writer = new StreamWriter(fileStream) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error init log file");
            }
        }
    }

    public void WriteCommand(string message)
    {
        if (this.writer != null)
        {
            lock (obj)
            {
                this.writer?.WriteLine(
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Command] {message}"
                );
            }
        }
    }

    public void WriteFeedback(string message)
    {
        if (this.writer != null)
        {
            lock (obj)
            {
                this.writer?.WriteLine(
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Feedback] {message}"
                );
            }
        }
    }

    public void Dispose()
    {
        this.writer?.Dispose();
    }
}