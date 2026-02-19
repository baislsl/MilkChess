using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MilkChess.Helper;

namespace MilkChess.Engine;

public class UcciLogger
{
    private readonly object obj = new();

    private readonly FileStream fileStream =
        new("app.log", FileMode.Append, FileAccess.Write, FileShare.Read);

    private readonly StreamWriter writer;

    public UcciLogger(IOptions<MilkChessConfig> config)
    {
        string logFile = config.Value.Engine.Log;
        this.fileStream = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.Read);
        this.writer = new StreamWriter(fileStream) { AutoFlush = true };
    }

    public void WriteCommand(string message)
    {
        lock (obj)
        {
            this.writer.WriteLine(
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Command] {message}"
            );
        }
    }

    public void WriteFeedback(string message)
    {
        lock (obj)
        {
            this.writer.WriteLine(
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Feedback] {message}"
            );
        }
    }

    public void Close()
    {
        this.writer.Close();
    }
}