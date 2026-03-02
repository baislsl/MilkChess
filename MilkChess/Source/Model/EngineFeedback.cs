using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MilkChess.Engine;

namespace MilkChess.Model;

public partial class EngineFeedback : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScoreText))]
    [NotifyPropertyChangedFor(nameof(ScoreColor))]
    private ObservableCollection<PredictMovementTrace> _predictMovementTraces;

    private int? EvalScore => this.PredictMovementTraces?.FirstOrDefault(t => t.MultiPv == 1)?.Score;

    public string ScoreText => EvalScore?.ToString() ?? string.Empty;

    public Microsoft.Maui.Graphics.Color ScoreColor =>
        (EvalScore.HasValue && this.EvalScore.Value > 0) ? Colors.Red : Colors.Black;

    public IAsyncRelayCommand<PredictMovementTrace> OpenPredictTracePreview;

    private PredictMovementTrace lastSelected;

    private DateTime lastClickTime;

    public EngineFeedback()
    {
        this.PredictMovementTraces = new ObservableCollection<PredictMovementTrace>();
        this.PredictMovementTraces.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ScoreText));
        this.PredictMovementTraces.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ScoreColor));
    }

    public void Insert(PredictMovementTrace trace)
    {
        this.PredictMovementTraces.Insert(0, trace);
    }

    public void Clear()
    {
        this.PredictMovementTraces.Clear();
    }


    [RelayCommand]
    private async Task OnSelectionChangedTaskAsync(PredictMovementTrace current)
    {
        if (current == null)
        {
            return;
        }

        var now = DateTime.UtcNow;

        if (lastSelected == current &&
            (now - lastClickTime).TotalMilliseconds < 350)
        {
            // 双击
            if (this.OpenPredictTracePreview != null)
            {
                await this.OpenPredictTracePreview.ExecuteAsync(current);
            }
        }
        else
        {
            if (lastSelected != null)
            {
                lastSelected.IsSelected = false;
            }

            current.IsSelected = true;
        }

        lastSelected = current;
        lastClickTime = now;
    }
}

public partial class PredictMovementTrace : ObservableObject
{
    public int Score { get; set; }

    public int Depth { get; set; }

    public string Movements { get; set; }

    public int MultiPv { get; set; } = 1; // Default 1


    public bool IsMultiPv { get; set; }

    public string MultiPvText => IsMultiPv ? $" MultiPV: {MultiPv}" : string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    public InfoFeedback RawInfoFeedback { get; set; }
}