using System.Collections;
using Microsoft.Extensions.Logging;
using MilkChess.Helper;
using MilkChess.Ucci;
using static MilkChess.Model.Color;

namespace MilkChess.Model;

public class ChessModel : IChessBoard
{
    public List<List<Chess>> board { get; set; }
    
    public ChessModel()
    {
        this.InternalReset();
    }

    public ChessModel(IChessBoard chessBoard)
    {
        this.board = new List<List<Chess>>(Constants.ChessBoardHeight);
        for (int i = 0; i < this.board.Capacity; i++)
        {
            this.board.Add(new List<Chess>(new Chess[Constants.ChessBoardWidth]));
        }

        Iterator.IterateOverChessBoard(position =>
        {
            SetChessAtPosition(chessBoard.GetChessAtPosition(position), position, out _);
        });
    }

    public bool HasChessAtPosition(Position position)
    {
        return this.GetChessAtPosition(position) != null;
    }

    public virtual Chess GetChessAtPosition(Position position)
    {
        if (position == null)
        {
            return null;
        }

        return this.board[position.Y][position.X];
    }

    public virtual void SetChessAtPosition(Chess chess, Position position, out Chess removedChess)
    {
        removedChess = this.GetChessAtPosition(position);
        this.board[position.Y][position.X] = chess;
    }

    public virtual void ApplyMovement(Movement movement)
    {
        SetChessAtPosition(null, movement.From, out _);
        SetChessAtPosition(movement.MovedChess, movement.To, out _);
    }

    public virtual void RevertMovement(Movement movement)
    {
        SetChessAtPosition(movement.MovedChess, movement.From, out _);
        SetChessAtPosition(movement.RemovedChess, movement.To, out _);
    }

    public virtual void MoveChess(Position from, Position to, out Chess removedChess)
    {
        Chess chess = this.GetChessAtPosition(from);
        this.SetChessAtPosition(chess, to, out removedChess);
    }

    public bool ValidateMovement(Position from, Position to)
    {
        if (from == to) return false;

        Chess moveChess = this.GetChessAtPosition(from);
        Chess overrideChess = this.GetChessAtPosition(to);

        if (moveChess == null) return false;
        if (overrideChess != null && moveChess.Color == overrideChess.Color)
        {
            return false;
        }

        foreach (var nextPosition in this.GetAvailableNextPosition(from))
        {
            if (nextPosition == to) return true;
        }

        return false;
    }

    public IEnumerable<Position> GetAvailableNextPosition(Position position)
    {
        int[][] dir = new int[][] { new[] { 0, 1 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { -1, 0 } };
        int[][] xyDir = new int[][] { new[] { 1, 1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { -1, -1 } };

        Chess moveChess = this.GetChessAtPosition(position);
        switch (moveChess.Name)
        {
            case ChessName.King:
                foreach (var d in dir)
                {
                    if (Position.IsLegalKingPositionXY(moveChess.Color, position.X + d[0], position.Y + d[1]))
                    {
                        yield return new Position(position.X + d[0], position.Y + d[1]);
                    }
                }

                yield break;
            case ChessName.Advisor:
                foreach (var d in xyDir)
                {
                    if (Position.IsLegalAdvisorPositionXY(moveChess.Color, position.X + d[0], position.Y + d[1]))
                    {
                        yield return new Position(position.X + d[0], position.Y + d[1]);
                    }
                }

                yield break;
            case ChessName.Bishop:
                foreach (var d in xyDir)
                {
                    if (Position.IsLegalBiShopPositionXY(moveChess.Color, position.X + 2 * d[0],
                            position.Y + 2 * d[1])
                        && !this.HasChessAtPosition(new Position(position.X + d[0], position.Y + d[1])))
                    {
                        yield return new Position(position.X + 2 * d[0], position.Y + 2 * d[1]);
                    }
                }

                yield break;
            case ChessName.Rook:
                foreach (var d in dir)
                {
                    for (int step = 1;; step++)
                    {
                        int nx = position.X + step * d[0], ny = position.Y + step * d[1];
                        if (Position.IsLegalXY(nx, ny))
                        {
                            yield return new Position(nx, ny);
                        }
                        else
                        {
                            break;
                        }

                        if (this.HasChessAtPosition(new Position(nx, ny)))
                        {
                            break;
                        }
                    }
                }

                yield break;
            case ChessName.Knight:
                foreach (var d in xyDir)
                {
                    int nx = position.X + d[0], ny = position.Y + 2 * d[1];
                    if (Position.IsLegalXY(nx, ny) && !this.HasChessAtPosition(new Position(nx - d[0], ny - d[1])))
                    {
                        yield return new Position(nx, ny);
                    }

                    nx = position.X + 2 * d[0];
                    ny = position.Y + d[1];
                    if (Position.IsLegalXY(nx, ny) && !this.HasChessAtPosition(new Position(nx - d[0], ny - d[1])))
                    {
                        yield return new Position(nx, ny);
                    }
                }

                yield break;
            case ChessName.Pawn:
                List<int[]> ds;
                if (moveChess.Color == RED)
                {
                    ds = new List<int[]> { new[] { 0, 1 } };
                    if (position.InColorArea(BLACK))
                    {
                        ds.Add(new[] { -1, 0 });
                        ds.Add(new[] { 1, 0 });
                    }
                }
                else
                {
                    ds = new List<int[]> { new[] { 0, -1 } };
                    if (position.InColorArea(RED))
                    {
                        ds.Add(new[] { -1, 0 });
                        ds.Add(new[] { 1, 0 });
                    }
                }

                foreach (var d in ds)
                {
                    if (Position.IsLegalXY(position.X + d[0], position.Y + d[1]))
                    {
                        yield return new Position(position.X + d[0], position.Y + d[1]);
                    }
                }

                yield break;
            case ChessName.Cannon:
                foreach (var d in dir)
                {
                    bool flag = false;
                    for (int step = 1;; step++)
                    {
                        int nx = position.X + step * d[0], ny = position.Y + step * d[1];
                        if (!Position.IsLegalXY(nx, ny)) break;

                        if (this.HasChessAtPosition(new Position(nx, ny)))
                        {
                            if (flag)
                            {
                                yield return new Position(nx, ny);
                                break;
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        else
                        {
                            if (!flag) yield return new Position(nx, ny);
                        }
                    }
                }

                yield break;
        }
    }

    public virtual void Reset()
    {
        this.InternalReset();
    }

    private void InternalReset()
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
                    this.board[black][i] = (Chess)this.board[red][i].Clone();
                    this.board[black][i].Color = BLACK;
                }
            }
        }
    }
}