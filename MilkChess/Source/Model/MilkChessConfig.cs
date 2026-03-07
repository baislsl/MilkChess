using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace MilkChess.Model;

public class MilkChessConfigService
{
    private readonly ILogger<MilkChessConfigService> logger;

    private readonly string filePath;

    private readonly JsonSerializerOptions options;

    public MilkChessConfigService(ILogger<MilkChessConfigService> logger)
    {
        this.filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "MilkChess", "MilkChessConfig.json");
        this.logger = logger;

        this.options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public MilkChessConfig Load()
    {
        this.logger.LogInformation("Load MilkChessConfig.json at {filePath}", this.filePath);

        if (!File.Exists(filePath))
        {
            return new MilkChessConfig();
        }

        var json = File.ReadAllText(filePath);

        this.logger.LogInformation("Getting config: {json}", json);

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
    [ObservableProperty]
    private string _name;

    public string Path { get; set; }

    public List<string> Arguments { get; set; }

    public int GoDepth { get; set; } = 25;

    public string Log { get; set; }

    public int ConnectionTimeoutSeconds { get; set; } = 20;

    public int Threads { get; set; } = 4;

    [ObservableProperty]
    private bool _isDefault;

    public int MultiPv { get; set; } = 1;

    public ObservableCollection<EngineOption> Options { get; set; } = new();

    public EngineConfig Clone() => new()
    {
        Name = Name,
        Path = Path,
        Arguments = Arguments,
        GoDepth = GoDepth,
        Log = Log,
        ConnectionTimeoutSeconds = ConnectionTimeoutSeconds,
        Threads = Threads,
        IsDefault = IsDefault,
        MultiPv = MultiPv,
        Options = new ObservableCollection<EngineOption>(Options),
    };
}

public class EngineOption
{
    public string Name { get; set; }

    public string Value { get; set; }
}