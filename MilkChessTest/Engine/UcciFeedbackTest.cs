using MilkChess.Engine;

namespace MilkChessTest.Engine;

[TestClass]
public class UcciFeedbackTest
{
    [TestMethod]
    public void TestPikafishEngineFeedback()
    {
        string feedback =
            "info depth 30 seldepth 41 multipv 1 score cp 19 nodes 3395797 nps 1367618 hashfull 866 tbhits 0 time 2483 pv e3d3 e5d5 d3e3 a4a5 a6a5 d5a5 b9c7 a5h5 b6f6 a2a5 c7a6 h5h9 e8f9 h9h4 a6c5 h4c4 e3d3 d0e1 f6e6 e0d0 f9e8 a5a2 d3e3 i3i4 e6e5 c4c3 e5e4 i4i5 e3d3 c3c2 c5e6 i5h5 e6f4";
        var result = UciFeedback.TryParse(feedback, out UciFeedback feedbackResult);
        Assert.IsTrue(result);
        Assert.AreEqual("19", feedbackResult.InfoDict["score"]);

        feedback =
            "info depth 1 seldepth 6 multipv 1 score mate 1 nodes 85 nps 85000 hashfull 0 tbhits 0 time 1 pv g8g0";

        result = UciFeedback.TryParse(feedback, out feedbackResult);
        Assert.IsTrue(result);
        Assert.AreEqual("29999", feedbackResult.InfoDict["score"]);
        Assert.AreEqual("1", feedbackResult.InfoDict["mate"]);

        feedback =
            "info depth 245 seldepth 3 multipv 1 score mate -1 nodes 980 nps 980000 hashfull 0 tbhits 0 time 1 pv d0d1 b4b1";
        result = UciFeedback.TryParse(feedback, out feedbackResult);
        Assert.IsTrue(result);
        Assert.AreEqual("29999", feedbackResult.InfoDict["score"]);
        Assert.AreEqual("-1", feedbackResult.InfoDict["mate"]);
    }

    [TestMethod]
    public void TestCycloneEngineFeedback()
    {
        string feedback =
            "info depth 12 seldepth 16 multipv 1 score 108 nodes 199378 nps 2932029 tbhits 0 time 68 pv b2e2 b9c7 g3g4 h7e7 b0c2 a9b9 a0b0 h9g7 h0g2 b7b3 c3c4 i9h9";
        var result = UciFeedback.TryParse(feedback, out UciFeedback feedbackResult);
        Assert.IsTrue(result);
        Assert.AreEqual("108", feedbackResult.InfoDict["score"]);
    }
}