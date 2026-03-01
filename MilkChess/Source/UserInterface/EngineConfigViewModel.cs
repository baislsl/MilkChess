using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MilkChess.Engine;
using MilkChess.Helper;

namespace MilkChess.UserInterface;

public partial class EngineConfigViewModel : ObservableObject
{
    private readonly MilkChessConfigService MilkChessConfigService;
    private readonly AlertService AlertService;

    public MilkChessConfig MilkChessConfig { get; set; }

    [ObservableProperty] private EngineConfig _selectedEngine;

    public EngineConfigViewModel(
        MilkChessConfigService milkChessConfigService,
        MilkChessConfig milkChessConfig,
        AlertService alertService)
    {
        AlertService = alertService;
        MilkChessConfigService = milkChessConfigService;
        MilkChessConfig = milkChessConfig;

        SelectedEngine = MilkChessConfig.Engines.FirstOrDefault(e => e.Name == MilkChessConfig.DefaultEngine);
    }

    [RelayCommand]
    private void AddEngine()
    {
        var engine = new EngineConfig { Name = "NewEngine" };
        MilkChessConfig.Engines.Add(engine);
        SelectedEngine = engine;
    }

    [RelayCommand]
    private void RemoveEngine(EngineConfig engine)
    {
        MilkChessConfig.Engines.Remove(engine);

        if (SelectedEngine == engine)
        {
            SelectedEngine = null;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        MilkChessConfigService.Save(this.MilkChessConfig);

        await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private void SetAsDefault()
    {
        if (SelectedEngine == null)
        {
            return;
        }

        MilkChessConfig.DefaultEngine = SelectedEngine.Name;
    }
}