using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using MilkChess.UserInterface;

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

public partial class MilkChessConfig : ObservableObject
{
    public static MilkChessConfig Create(MilkChessConfigService service)
    {
        return service.Load();
    }

    public ObservableCollection<EngineConfig> Engines { get; set; } = [];

    public Dictionary<string, string> EngineOption { get; set; } = new();

    [ObservableProperty]
    private string _defaultEngine = string.Empty;

    public string LastChessBoardFen { get; set; } = string.Empty;

    [JsonIgnore]
    public EngineConfig DefaultEngineConfig => Engines.FirstOrDefault(e => e.Name == DefaultEngine);

    public bool IsRotated { get; set; } = false;

    public double ChessBoardScale { get; set; } = 1.0;

    partial void OnDefaultEngineChanged(string oldValue, string newValue)
    {
        foreach (var engine in Engines)
        {
            if (engine.Name == oldValue)
            {
                engine.IsDefault = false;
            }

            if (engine.Name == newValue)
            {
                engine.IsDefault = true;
            }
        }
    }
}

public partial class EngineConfig : ObservableObject
{
    public string Name { get; set; }
    public string Path { get; set; }

    public List<string> Arguments { get; set; }

    public int GoDepth { get; set; } = 25;

    public string Log { get; set; }

    public int ConnectionTimeoutSeconds { get; set; } = 20;

    public int Threads { get; set; } = 4;

    [ObservableProperty]
    private bool _isDefault;

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
