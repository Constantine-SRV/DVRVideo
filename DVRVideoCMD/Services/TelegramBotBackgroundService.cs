using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Runs TelegramUpdateListener in a background hosted service.
/// </summary>
public class TelegramBotBackgroundService : BackgroundService
{
    private readonly TelegramUpdateListener _listener;

    public TelegramBotBackgroundService(TelegramCommandRouter commandHandler)
    {
        _listener = new TelegramUpdateListener(AppSettingsService.TelegramToken);
        _listener._commandHandler += commandHandler.HandleAnyCommand;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[INFO] Telegram bot background service started.");
        await _listener.StartListeningAsync();
    }
}
