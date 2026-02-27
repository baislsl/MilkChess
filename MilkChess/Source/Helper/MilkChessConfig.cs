namespace MilkChess.Helper;

public class MilkChessConfig
{
    public Engine Engine { get; set; }

    public Dictionary<string, string> EngineOption { get; set; } = new();

    public PathDefinition PathDefinition { get; set; }
}

public class Engine
{
    public string Path { get; set; }

    public List<string> Arguments { get; set; }

    public int GoDepth { get; set; }

    public string Log { get; set; }

    public int ConnectionTimeoutSeconds { get; set; }

}

public class PathDefinition
{
    public string Root { get; set; }

    public string DataPath { get; set; }
}