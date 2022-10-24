using Microsoft.Extensions.Logging;
using MilkChess.UserInterface;

namespace MilkChess
{
    using Ucci;

    public static class MauiProgram
    {
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
                configure.AddDebug();
            });
            
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<BoardDrawable>();

            return builder.Build();
        }
    }
}