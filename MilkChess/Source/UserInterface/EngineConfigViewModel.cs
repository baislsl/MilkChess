using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MilkChess.Engine;
using MilkChess.Helper;

namespace MilkChess.UserInterface;

public partial class EngineConfigViewModel : ObservableObject
{
    private readonly EngineManager EngineManager;

    public ObservableCollection<EngineConfig> Engines { get; }

    [ObservableProperty] private EngineConfig? selectedEngine;

    public EngineConfigViewModel(EngineManager engineManager)
    {
        EngineManager = engineManager;

        Engines = new ObservableCollection<EngineConfig>(
            engineManager.Engines.Select(e => e.Clone()));

        SelectedEngine = Engines.FirstOrDefault(e => e.Name == engineManager.Config.DefaultEngine);
    }

    [RelayCommand]
    private void AddEngine()
    {
        var engine = new EngineConfig { Name = "NewEngine" };
        Engines.Add(engine);
        SelectedEngine = engine;
    }

    [RelayCommand]
    private void RemoveEngine(EngineConfig engine)
    {
        Engines.Remove(engine);

        if (SelectedEngine == engine)
        {
            SelectedEngine = null;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        EngineManager.Save(
            Engines,
            SelectedEngine?.Name);

        await Application.Current.MainPage.Navigation.PopModalAsync();
    }
}