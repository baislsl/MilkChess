using MilkChess.Helper;

namespace MilkChess.Engine;

public class EngineManager
{
    private readonly MilkChessConfigService MilkChessConfigService;

    public MilkChessConfig Config { get; private set; }

    public IReadOnlyList<EngineConfig> Engines => Config.Engines;

    public EngineManager(MilkChessConfigService milkChessConfigService, MilkChessConfig config)
    {
        this.MilkChessConfigService = milkChessConfigService;
        this.Config = config;
    }

    public void Save(IEnumerable<EngineConfig> engines, string? defaultEngine = null)
    {
        Config.Engines.Clear();
        Config.Engines.AddRange(engines);
        Config.DefaultEngine = defaultEngine;

        MilkChessConfigService.Save(Config);
    }
}