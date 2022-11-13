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
}