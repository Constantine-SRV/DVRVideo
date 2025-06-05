using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class TelegramBotBackgroundService : BackgroundService
{
    private readonly TelegramBotListener _listener;

    public TelegramBotBackgroundService(TelegramCommandHandler commandHandler)
    {
        _listener = new TelegramBotListener(AppSettingsService.TelegramToken);
        _listener._commandHandler += commandHandler.HandleAnyCommand;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[INFO] Telegram bot background service started.");
        await _listener.StartListeningAsync();
    }
}
