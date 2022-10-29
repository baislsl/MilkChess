using System.Collections;
using MilkChess.Ucci;
using static MilkChess.Source.Model.Color;

namespace MilkChess.Source.Model;

public class ChessModel
{
    public static ChessModel Instance = new ChessModel();

    public Cursor Cursor { get; internal set; }

    public List<List<Chess>> board { get; set; }

    public ChessModel()
    {
        this.Cursor = new Cursor(new Position(0, 0));
        this.Reset();
    }

    public void Initialize(List<List<Chess>> board)
    {
        this.board = board;
    }

    public Chess GetChessAtPosition(Position position)
    {
        return this.board[position.Y][position.X];
    }

    public void SetChessAtPosition(Chess chess, Position position, out Chess removedChess)
    {
        removedChess = this.GetChessAtPosition(position);
        this.board[position.Y][position.X] = chess;
    }

    public void MoveChess(Position from, Position to, out Chess removedChess)
    {
        Chess chess = this.GetChessAtPosition(from);
        this.SetChessAtPosition(chess, to, out removedChess);
    }

    public Cursor MoveCursor(Position position)
    {
        this.Cursor.Position = position;
        return this.Cursor;
    }

    public void Reset()
    {
        this.board = new List<List<Chess>>(Constants.ChessBoardHeight);
        for (int i = 0; i < this.board.Capacity; i++)
        {
            this.board.Add(new List<Chess>(new Chess[Constants.ChessBoardWidth]));
        }

        this.board[0] = new List<Chess>
        {
            new Rook(RED), new Knight(RED), new Bishop(RED), new Advisor(RED),
            new King(RED),
            new Advisor(RED), new Bishop(RED), new Knight(RED), new Rook(RED)
        };

        this.board[2] = new List<Chess>
        {
            null, new Cannon(RED), null, null, null, null, null, new Cannon(RED), null
        };

        this.board[3] = new List<Chess>
        {
            new Pawn(RED), null, new Pawn(RED), null,
            new Pawn(RED),
            null, new Pawn(RED), null, new Pawn(RED)
        };

        // Initialize BLACK by copying from RED chess 
        for (int i = 0; i < Constants.ChessBoardWidth; i++)
        {
            for (int red = 0, black = Constants.ChessBoardHeight - 1; red < black; red++, black--)
            {
                if (this.board[red][i] != null)
                {
                    this.board[black][i] = (Chess) this.board[red][i].Clone();
                    this.board[black][i].Color = BLACK;
                }
            }
        }
    }
}