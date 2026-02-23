using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MilkChess.Model;

public partial class EngineFeedback : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScoreText))]
    [NotifyPropertyChangedFor(nameof(ScoreColor))]
    private ObservableCollection<PredictMovementTrace> _predictMovementTraces;

    private int? EvalScore => this.PredictMovementTraces?.FirstOrDefault()?.Score;

    public string ScoreText => EvalScore?.ToString() ?? string.Empty;

    public Microsoft.Maui.Graphics.Color ScoreColor =>
        (EvalScore.HasValue && this.EvalScore.Value > 0) ? Colors.Red : Colors.Black;

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
}

public class PredictMovementTrace
{
    public int Score { get; set; }

    public int Depth { get; set; }

    public string Movements { get; set; }
}