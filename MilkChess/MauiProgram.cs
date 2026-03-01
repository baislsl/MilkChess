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
#if !(IOS || ANDROID || MACCATALYST || WINDOWS)
        public static void Main(string[] args)
        {
        }
#endif
        public static MauiApp CreateMauiApp()
        {
            // Resolve System.Text.Json serialize or deserialize exception
            AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);

            var builder = MauiApp.CreateBuilder()
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
            builder.Services.AddSingleton<HistoryManager>();
            builder.Services.AddTransient<UciLogger>();
            builder.Services.AddTransient<UciEngine>();
            builder.Services.AddTransient<EngineFeedback>();
            builder.Services.AddTransient<LocalExecutableEngine>();
            builder.Services.AddTransient<IdleChessPanel>();
            builder.Services.AddTransient<EditChessBoardPage>();
            builder.Services.AddTransient<EditChessBoardViewModel>();
            builder.Services.AddTransient<MilkChessConfigService>();
            builder.Services.AddSingleton<MilkChessConfig>(serviceProvider =>
                MilkChessConfig.Create(serviceProvider.GetRequiredService<MilkChessConfigService>()));
            builder.Services.AddTransient<EngineManager>();
            builder.Services.AddTransient<EngineConfigViewModel>();
            builder.Services.AddTransient<EngineConfigPage>();
            builder.Services.AddTransient<AlertService>();
            builder.Services.AddSingleton<GlobalConfig>(serviceProvider =>
                GlobalConfig.Instance.Init(serviceProvider.GetRequiredService<MilkChessConfig>()));

            return builder.Build();
        }
    }
}