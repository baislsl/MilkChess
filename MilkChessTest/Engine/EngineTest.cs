using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MilkChess;
using MilkChess.Engine;
using MilkChess.Helper;

namespace MilkChessTest.Engine;

[TestClass]
public class EngineTest
{
    private const string LocalExecutableFile =
        "/Users/baislsl/Documents/chess/Pikafish.2026-01-02/MacOS/pikafish-apple-silicon";

    [TestMethod]
    public void TestRemoteEngine()
    {
        var app = MauiProgram.CreateMauiApp();

        var engine = new LocalExecutableEngine(LocalExecutableFile, null, app.Services.GetService<UcciLogger>());
        engine.Initialize();
        engine.DiscardBufferData();
        engine.SendCommand("uci");
        string ret = null;
        for (int cnt = 0; cnt < 100 && ret != "uciok"; cnt++)
        {
            ret = engine.GetFeedback();
            Console.WriteLine($"Ret: {ret}.");
        }

        Assert.AreEqual("uciok", ret);
    }

    [TestMethod]
    public void TestUcciEngine()
    {
        var app = MauiProgram.CreateMauiApp();

        UcciEngine engine = new UcciEngine(
            app.Services.GetService<ILogger<UcciEngine>>(),
            app.Services.GetService<IOptions<MilkChessConfig>>(),
            app.Services.GetService<UcciLogger>());

        engine.StartUci(CancellationToken.None);
        engine.WaitToReady(CancellationToken.None);
        engine.Position(new MockChessModel(app.Services), CancellationToken.None);
        var goOption = new GoOption()
        {
            Depth = 10,
        };
        engine.Go(goOption, CancellationToken.None, null);
        engine.Quit();
    }
}