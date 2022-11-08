using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using MilkChess;
using MilkChess.Engine;

namespace MilkChessTest.Engine;

[TestClass]
public class EngineTest
{
    private const string LocalExecutableFile =
                    "/Users/baislsl/go/src/github.com/baislsl/UcciProxy/client/remote_engine";

    private Collection<string> Arguments = new Collection<string> {"-addr", "192.168.31.7:7890"};

    [TestMethod]
    public void TestRemoteEngine()
    {
        var app = MauiProgram.CreateMauiApp();

        LocalExecutableEngine engine = new LocalExecutableEngine(LocalExecutableFile, Arguments);
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
                        new UcciEngineConfig()
                        {
                                        LocalExecutableFile = LocalExecutableFile,
                                        Arguments = Arguments,
                        });

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