using MilkChess;
using MilkChess.Model;
using MilkChess.Ucci;
using static MilkChess.Model.Color;

namespace MilkChessTest;

[TestClass]
public class ForsythEdwardsNotationTest
{
    [TestMethod]
    public void TestReadFEN()
    {
        var app = MauiProgram.CreateMauiApp();

        string fen = "rnbakabnr/9/4c4/p1p1p1p1p/9/9/P1P1P1P1P/1C3CN2/9/1RBAKAB1R b - - 0 1";
        bool ret = ForsythEdwardsNotation.ParseString(fen, out ForsythEdwardsNotation notation);
        Assert.IsTrue(ret);
        Assert.AreEqual(BLACK, notation.ActiveColor);

        IChessBoardHandler chessBoardHandler = new MockChessModel(app.Services);
        chessBoardHandler.AdoptForsythEdwardsNotation(notation);

        Assert.IsNull(chessBoardHandler.GetChessAtPosition(new Position(0, 0)));
        Assert.AreEqual(ChessDefinition.Rook.Name, chessBoardHandler.GetChessAtPosition(new Position(1, 0)).Name);
        Assert.AreEqual(ChessDefinition.Cannon.Name, chessBoardHandler.GetChessAtPosition(new Position(1, 2)).Name);
        Assert.AreEqual(RED, chessBoardHandler.GetChessAtPosition(new Position(1, 2)).Color);
        Assert.AreEqual(ChessDefinition.Cannon.Name, chessBoardHandler.GetChessAtPosition(new Position(4, 7)).Name);
        Assert.AreEqual(BLACK, chessBoardHandler.GetChessAtPosition(new Position(4, 7)).Color);

    }
    
}