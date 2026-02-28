using System.Text.Json;
using System.Text.Json.Serialization;

namespace MilkChess.Helper;

public class MilkChessConfigService
{
    private readonly string filePath;

    private readonly JsonSerializerOptions options;

    public MilkChessConfigService()
    {
        filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "MilkChess", "MilkChessConfig.json");

        options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public MilkChessConfig Load()
    {
        if (!File.Exists(filePath))
        {
            return new MilkChessConfig();
        }

        var json = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<MilkChessConfig>(json) ?? new MilkChessConfig();
    }

    public void Save(MilkChessConfig config)
    {
        var json = JsonSerializer.Serialize(config, options);

        File.WriteAllText(filePath, json);
    }
}

public class MilkChessConfig
{
    public static MilkChessConfig Create(MilkChessConfigService service)
    {
        return service.Load();
    }

    public List<EngineConfig> Engines { get; set; } = [];

    public Dictionary<string, string> EngineOption { get; set; } = new();

    public string DefaultEngine { get; set; } = string.Empty;

    public string LastChessBoardFen { get; set; } = string.Empty;

    [JsonIgnore] public EngineConfig EngineConfig => Engines.FirstOrDefault(e => e.Name == DefaultEngine);
}

public class EngineConfig
{
    public string Name { get; set; }
    public string Path { get; set; }

    public List<string> Arguments { get; set; }

    public int GoDepth { get; set; } = 25;

    public string Log { get; set; }

    public int ConnectionTimeoutSeconds { get; set; } = 20;

    public int Threads { get; set; } = 4;

    public EngineConfig Clone() => new()
    {
        Name = Name,
        Path = Path,
        Arguments = Arguments,
        GoDepth = GoDepth,
        Log = Log,
        ConnectionTimeoutSeconds = ConnectionTimeoutSeconds,
        Threads = Threads
    };
}

public class AppSettings
{
}