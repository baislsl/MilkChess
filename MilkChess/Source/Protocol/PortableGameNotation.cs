using MilkChess.Model;

namespace MilkChess.Protocol;

/**
 * PGN definition
 */
public class PortableGameNotation : IChessManualTranslator
{
    public void LoadFromStream(Stream file, IChessBoardContext chessBoardContext)
    {
        throw new NotImplementedException();
    }

    public void SaveToStream(Stream file, IChessBoardContext chessBoardContext)
    {
        throw new NotImplementedException();
    }
}