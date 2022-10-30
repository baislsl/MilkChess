using MilkChess.Model;

namespace MilkChess.Ucci;

/**
 * PGN definition
 */
public class PortableGameNotation : IChessManualTranslator
{
    public void LoadFromStream(Stream file, IChessBoardHandler chessBoardHandler)
    {
        throw new NotImplementedException();
    }

    public void SaveToStream(Stream file, IChessBoardHandler chessBoardHandler)
    {
        throw new NotImplementedException();
    }
}