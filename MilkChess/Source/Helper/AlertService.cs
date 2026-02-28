using Microsoft.Extensions.Logging;

namespace MilkChess.Helper;

public class AlertService
{
    public ILogger<AlertService> Logger;

    public AlertService(ILogger<AlertService> logger)
    {
        this.Logger = logger;
    }

    public async Task PopAlertAsync(string message)
    {
        Logger.LogError("Pop alert {message}", message);

        if (Application.Current != null && Application.Current.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("MilkChess", message, "OK");
        }
        else
        {
            this.Logger.LogError("Application current is null");
        }
    }
}