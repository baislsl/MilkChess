using System.Xml.Serialization;
using MilkChess.Helper;
using MilkChess.Model;

namespace MilkChess.Ucci;

public class JsonChessManualTranslator : IChessManualTranslator
{
    public void LoadFromStream(Stream file, IChessBoardContext chessBoardContext)
    {
        ChessModel chessModel = new ChessModel();

        XmlSerializer serializer = new XmlSerializer(typeof(UnifyChessHistory));
        UnifyChessHistory chessHistory = (UnifyChessHistory)serializer.Deserialize(file);

        chessBoardContext.HistoryManager.Reset();

        foreach (var record in chessHistory.records)
        {
            if (!chessModel.ParseUnifyStringMovement(record.Move, out Movement movement))
            {
                throw new ArgumentException($"Invalid move {record.Move}");
            }

            chessModel.ApplyMovement(movement);

            chessBoardContext.HistoryManager.InsertMovement(movement);
        }

        Iterator.IterateOverChessBoard(position =>
        {
            chessBoardContext.SetChessAtPosition(chessModel.GetChessAtPosition(position), position,
                out _);
        });
    }

    public void SaveToStream(Stream file, IChessBoardContext chessBoardContext)
    {
        HistoryManager historyManager = chessBoardContext.HistoryManager;

        UnifyChessHistory unifyChessHistory = new UnifyChessHistory();

        int counter = 0;
        foreach (var movement in historyManager.GetMovementHistory())
        {
            counter++;
            unifyChessHistory.records.Add(new Record()
            {
                Move = movement.ToUnifyString(),
                Count = counter,
                ViewableString = movement.ChineseViewableString
            });
        }

        XmlSerializer serializer = new XmlSerializer(typeof(UnifyChessHistory));
        serializer.Serialize(file, unifyChessHistory);
    }

    private class Record
    {
        public string Move;
        public string ViewableString;
        public int Count;
    }

    private class UnifyChessHistory
    {
        public List<Record> records;

        public UnifyChessHistory()
        {
            this.records = new List<Record>();
        }
    }
}