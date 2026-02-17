using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkChess.Engine;
using MilkChess.Helper;
using MilkChess.Model;
using MilkChess.UserInterface;

namespace MilkChess
{
    public static class MauiProgram
    {
        private const string LocalExecutableFile =
                        "/Users/baislsl/go/src/github.com/baislsl/UcciProxy/listener/listener";

        private static Collection<string> Arguments = new Collection<string>
        {
                        // "-addr", "192.168.31.6:8123",
                        //"-output", "/private/tmp/remote_engine.log"
        };

        private const string ConfigResourcePath = "MilkChess.Resources.Config.appsettings.ini";


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
            builder.Services.AddTransient<ChessModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<ChessBoardViewModel>();
            builder.Services.AddSingleton<HistoryManager>(x =>
                            new HistoryManager(x.GetRequiredService<ILogger<HistoryManager>>(),
                                            new ObservableCollection<Movement>()));
            builder.Services.AddTransient<UcciEngineConfig>(_ =>
                            new UcciEngineConfig()
                            {
                                            LocalExecutableFile = LocalExecutableFile,
                                            Arguments = Arguments,
                            });
            builder.Services.AddTransient<UcciLogger>();
            builder.Services.AddTransient<UcciEngine>();

            using var stream = Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream(ConfigResourcePath);
            var config = new ConfigurationBuilder().AddIniStream(stream).Build();
            Globals.Config = config;

            builder.Configuration.AddConfiguration(config);

            return builder.Build();
        }
    }
}