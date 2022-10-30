using Microsoft.Extensions.Logging;
using MilkChess.Model;
using MilkChess.UserInterface;

namespace MilkChess
{
    public static class MauiProgram
    {
#if !(IOS || ANDROID || MACCATALYST || WINDOWS)
        public static void Main(string[] args)
        {
        }
#endif
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddLogging(configure => { configure.AddConsole(); });
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<ChessBoardViewModel>();
            builder.Services.AddSingleton<ChessModel>();
            builder.Services.AddSingleton<HistoryManager>();


            return builder.Build();
        }
    }
}