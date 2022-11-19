using Microsoft.Extensions.Logging;
using MilkChess;
using MilkChess.Model;
using MilkChess.Ucci;

namespace MilkChessTest;

[TestClass]
public class ChessModelTest
{
    [TestMethod]
    public void TestInitialize()
    {
        var app = MauiProgram.CreateMauiApp();
        ChessModel chessModel = new ChessModel();

        Assert.AreEqual(chessModel.board.Count, Constants.ChessBoardHeight);
        Assert.AreEqual(chessModel.board[0].Count, Constants.ChessBoardWidth);

        Assert.IsNotNull(chessModel.GetChessAtPosition(new Position(0, 0)));
    }
}