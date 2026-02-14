using Microsoft.Extensions.Configuration;

namespace MilkChess.Helper;

public static class Globals
{
    public static IConfiguration Config;
}

public class Keys
{
    public const string LayoutIsRotated = "Layout:IsRotated";

    public const string EngineGoDepth = "Engine:GoDepth";
    public const string EngineLog =  "Engine:Log";

    public const string PathRoot = "PathDefinition:Root";
    public const string DataPath = "PathDefinition:DataPath";
}