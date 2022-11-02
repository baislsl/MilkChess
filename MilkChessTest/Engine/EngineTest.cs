using System.Diagnostics;
using System.Net.WebSockets;

namespace MilkChessTest.Engine;

[TestClass]
public class EngineTest
{
    [TestMethod]
    public void TestEngine()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("IExplore.exe");
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        var p = Process.Start(startInfo);
        p.OutputDataReceived += null;
        p.Start();
        p.StandardInput.Write("123");
        p.StandardInput.Write("123");
    }
}