using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MilkChess.Helper;

namespace MilkChess.Engine;

public class UciLogger
{
    private readonly object obj = new();

    private readonly FileStream fileStream;

    private readonly StreamWriter writer;

    public UciLogger(IOptions<MilkChessConfig> config)
    {
        string logFile = config.Value.Engine.Log;

        if (!string.IsNullOrWhiteSpace(logFile))
        {
            this.fileStream = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.Read);
            this.writer = new StreamWriter(fileStream) { AutoFlush = true };
        }
    }

    public void WriteCommand(string message)
    {
        lock (obj)
        {
            this.writer?.WriteLine(
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Command] {message}"
            );
        }
    }

    public void WriteFeedback(string message)
    {
        lock (obj)
        {
            this.writer?.WriteLine(
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Feedback] {message}"
            );
        }
    }

    public void Dispose()
    {
        this.writer?.Dispose();
    }
}