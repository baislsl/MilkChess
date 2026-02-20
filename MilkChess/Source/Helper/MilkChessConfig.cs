namespace MilkChess.Helper;

public class MilkChessConfig
{
    public EngineConfig Engine { get; set; }

    public PathDefinitionConfig PathDefinition { get; set; }

    public class EngineConfig
    {
        public string Path { get; set; }

        public List<string> Arguments { get; set; }

        public int GoDepth { get; set; }

        public string Log { get; set; }
    }

    public class PathDefinitionConfig
    {
        public string Root { get; set; }

        public string DataPath { get; set; }
    }
}