using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Model;
using MilkChess.UserInterface;

namespace MilkChess
{
    public static class MauiProgram
    {
        private const string LocalExecutableFile =
                        "/Users/baislsl/go/src/github.com/baislsl/UcciProxy/client/remote_engine";

        private static Collection<string> Arguments = new Collection<string>
        {
                        "-addr", "192.168.31.7:7890",
                        "-output", "/private/tmp/remote_engine.log"
        };


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

            builder.Services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Debug);
            });
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<ChessBoardViewModel>();
            builder.Services.AddSingleton<HistoryManager>(x =>
                            new HistoryManager(x.GetRequiredService<ILogger<HistoryManager>>(),
                                            new ObservableCollection<Movement>()));
            builder.Services.AddTransient<UcciEngineConfig>(x =>
                            new UcciEngineConfig()
                            {
                                            LocalExecutableFile = LocalExecutableFile,
                                            Arguments = Arguments,
                            });
            builder.Services.AddTransient<UcciEngine>();
            return builder.Build();
        }
    }
}