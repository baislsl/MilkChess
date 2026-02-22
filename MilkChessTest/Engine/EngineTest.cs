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
    [TestMethod]
    public void TestRemoteEngine()
    {
        var app = MauiProgram.CreateMauiApp();

        var engine = new LocalExecutableEngine(
            app.Services.GetService<IOptions<MilkChessConfig>>(),
            app.Services.GetService<ILogger<LocalExecutableEngine>>(),
            app.Services.GetService<UcciLogger>());
        engine.Initialize();
        engine.SendCommand("uci");
        string ret = null;
        for (int cnt = 0; cnt < 100 && ret != "uciok"; cnt++)
        {
            ret = engine.GetFeedback()?.RawCommand;
            Console.WriteLine($"Ret: {ret}.");
        }

        Assert.AreEqual("uciok", ret);
    }
}