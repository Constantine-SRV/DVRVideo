using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

public class ABBPowerBackgroundService : BackgroundService
{
    private readonly ZabbixSenderService _zabbixSender;
    private readonly TimeSpan _interval;

    public ABBPowerBackgroundService(ZabbixSenderService zabbixSender)
    {
        _zabbixSender = zabbixSender;
        _interval = TimeSpan.FromMinutes(1); // Можно потом вынести в настройки
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[INFO] ABB power background service started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            var start = DateTime.UtcNow;
            try
            {
                var power = await ABBPowerService.GetPowerValuesAsync();
                if (power != null)
                {
                    // Отправляем все значения по схеме "power.abb.pin1" и т.д.
                    await SendPowerValuesToZabbixAsync(power);
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Power sent to Zabbix.");
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WARN] ABB inverter data unavailable.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] {ex.Message}");
            }

            // Умный задержка: ждем только оставшееся время до нового интервала
            var elapsed = DateTime.UtcNow - start;
            var delay = _interval - elapsed;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task SendPowerValuesToZabbixAsync(PowerValues power)
    {
        // Все host = "host-home"
        string host = "homeCY";

        // Используем CultureInfo.InvariantCulture, чтобы не было запятой вместо точки
        await _zabbixSender.SendAsync(host, "power.abb.pin1", power.Pin1.ToString(CultureInfo.InvariantCulture));
        await _zabbixSender.SendAsync(host, "power.abb.pin2", power.Pin2.ToString(CultureInfo.InvariantCulture));
        await _zabbixSender.SendAsync(host, "power.abb.pgrid", power.Pgrid.ToString(CultureInfo.InvariantCulture));
        await _zabbixSender.SendAsync(host, "power.abb.temp1", power.Temp1.ToString(CultureInfo.InvariantCulture));
        await _zabbixSender.SendAsync(host, "power.abb.e0_runtime", power.E0_runtime.ToString(CultureInfo.InvariantCulture));
        await _zabbixSender.SendAsync(host, "power.abb.fgrid", power.Fgrid.ToString(CultureInfo.InvariantCulture));
        // Если будут ещё параметры — просто добавь здесь!
    }
}
