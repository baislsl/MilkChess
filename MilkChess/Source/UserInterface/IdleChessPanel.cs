using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MilkChess.Helper;
using MilkChess.Model;
using Color = MilkChess.Model.Color;

namespace MilkChess.UserInterface;

public partial class IdleChessPanel : ObservableObject
{
    [ObservableProperty] private List<IdleChess> _idleChessList = new List<IdleChess>();


    private IdleChess _cursor;

    public IdleChess Cursor
    {
        get => _cursor;
        set
        {
            if (_cursor != null)
            {
                _cursor.IsSelected = false;
            }

            _cursor = value;

            if (_cursor != null)
            {
                _cursor.IsSelected = true;
            }
        }
    }

    private ILogger<IdleChessPanel> Logger;

    public IdleChessPanel(
        ILogger<IdleChessPanel> logger)
    {
        Logger = logger;

        var tmpChessModel = new ChessModel();
        Iterator.IterateOverChessBoard(position =>
        {
            var chess = tmpChessModel.GetChessAtPosition(position);
            if (chess != null && chess is not King)
            {
                IdleChessList.Add(new IdleChess()
                {
                    Chess = chess,
                    IsIdle = true,
                    Index = IdleChessList.Count
                });
            }
        });


        IdleChessList.Sort((a, b) =>
        {
            if (a.Chess.Color != b.Chess.Color)
            {
                return a.Chess.Color == Color.RED ? 1 : -1;
            }

            if (a.Chess.ChessDefinition.Number != b.Chess.ChessDefinition.Number)
            {
                return a.Chess.ChessDefinition.Number.CompareTo(b.Chess.ChessDefinition.Number);
            }

            return 0;
        });
    }

    public void InsertChess(Chess chess)
    {
        foreach (var chessBucket in IdleChessList)
        {
            if (chessBucket.Chess.Equals(chess) && !chessBucket.IsIdle)
            {
                chessBucket.IsIdle = true;
                return;
            }
        }

        throw new Exception($"Not available bucket {chess}");
    }

    public void RemoveChess(Chess chess)
    {
        foreach (var chessBucket in IdleChessList)
        {
            if (chessBucket.Chess.Equals(chess) && chessBucket.IsIdle)
            {
                chessBucket.IsIdle = false;
                return;
            }
        }

        throw new Exception($"Not available {chess} to remove");
    }

    public void Clear()
    {
        foreach (var chess in IdleChessList)
        {
            chess.IsIdle = false;
        }
    }

    public void Reset()
    {
        foreach (var chess in IdleChessList)
        {
            chess.IsIdle = true;
        }
    }

    public void ResetCursor()
    {
        this.Cursor = null;
    }
}