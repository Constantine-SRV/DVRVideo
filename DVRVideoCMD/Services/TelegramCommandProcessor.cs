using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class TelegramCommandProcessor
/// <summary>
/// Implements concrete Telegram bot commands. Invoked by TelegramCommandRouter.
/// </summary>
{
    private readonly WaterLevelAiAnalyzer _analyzer;
    private readonly ZabbixApiClient _zabbix;

    public TelegramCommandProcessor(WaterLevelAiAnalyzer analyzer, ZabbixApiClient zabbix)
    {
        _analyzer = analyzer;
        _zabbix = zabbix;
    }

    public async Task HandleSwim(long chatId, string cmd)
    {
        var images = new List<string>();
        foreach (var area in AppSettingsService.SnapshotAreas)
        {
            string tempCropPath = CameraSnapshotCache.GetOrCreateSnapshot(
                area.Channel,
                cacheSeconds: 10,
                cropArea: (area.X1, area.Y1, area.X2, area.Y2)
            );
            if (!string.IsNullOrEmpty(tempCropPath))
            {
                string camName = AppSettingsService.GetCameraName(area.Channel);
                await TelegramMessageSender.SendPhotoAsync(AppSettingsService.TelegramToken, chatId, tempCropPath, $"{camName} crop");
                images.Add(tempCropPath);
            }
        }

        if (images.Count == 2)
        {
            var percent = await _analyzer.AnalyzeWaterLevelAsync(images[0], images[1], chatId);
            await TelegramMessageSender.SendMessageAsync(AppSettingsService.TelegramToken, chatId, $"Water level: {percent}");
        }
        else
        {
            await TelegramMessageSender.SendMessageAsync(AppSettingsService.TelegramToken, chatId, "Error: Not enough camera images for analysis.");
        }
    }
    public async Task HandleChannel(long chatId, string cmd)
    {
        // Привести к нижнему регистру и убрать пробелы для унификации
        var cleanCmd = cmd.Replace(" ", "").ToLowerInvariant();

        // Каналы для отправки
        List<int> channels = new List<int>();

        // ch(all) или chall
        if (Regex.IsMatch(cleanCmd, @"^(ch|cam)(all|\(all\))$"))
        {
            channels = Enumerable.Range(1, 16).ToList();
        }
        // ch(n-m) или cam(n-m)
        else if (Regex.IsMatch(cleanCmd, @"(ch|cam)\((\d+)-(\d+)\)"))
        {
            var match = Regex.Match(cleanCmd, @"\((\d+)-(\d+)\)");
            if (match.Success)
            {
                int from = int.Parse(match.Groups[1].Value);
                int to = int.Parse(match.Groups[2].Value);
                if (from <= to && from >= 1 && to <= 16)
                    channels = Enumerable.Range(from, to - from + 1).ToList();
            }
        }
        // ch(n1,n2,...) или cam(n1,n2,...)
        else if (Regex.IsMatch(cleanCmd, @"(ch|cam)\(([\d,]+)\)"))
        {
            var match = Regex.Match(cleanCmd, @"\(([\d,]+)\)");
            if (match.Success)
            {
                channels = match.Groups[1].Value
                    .Split(',')
                    .Select(s => int.TryParse(s, out int n) ? n : -1)
                    .Where(n => n >= 1 && n <= 16)
                    .Distinct()
                    .ToList();
            }
        }
        // Просто chN или camN
        else if (Regex.IsMatch(cleanCmd, @"(ch|cam)(\d+)"))
        {
            var match = Regex.Match(cleanCmd, @"(ch|cam)(\d+)");
            if (match.Success && int.TryParse(match.Groups[2].Value, out int ch) && ch >= 1 && ch <= 16)
            {
                channels.Add(ch);
            }
        }

        // Если ничего не найдено — по умолчанию канал 2 (старое поведение)
        if (channels.Count == 0)
            channels.Add(1);

        // Отправляем фото для каждого канала
        foreach (var ch in channels)
        {
            string tempCropPath = CameraSnapshotCache.GetOrCreateSnapshot(
                ch,
                cacheSeconds: 10,
                cropArea: (0, 0, 10000, 10000)
            );
            if (!string.IsNullOrEmpty(tempCropPath))
            {
                string camName = AppSettingsService.GetCameraName(ch);
                await TelegramMessageSender.SendPhotoAsync(AppSettingsService.TelegramToken, chatId, tempCropPath, $"{camName}");
            }
        }
    }

    public async Task HandleHomeOrTemp(long chatId, string cmd)
    {
        try
        {
            var items = await _zabbix.GetLastValuesAsync("homeCY");
            var message = TelegramTextFormatter.FormatLastValuesAsBlocks(items);
            await TelegramMessageSender.SendMessageAsync(AppSettingsService.TelegramToken, chatId, message, parseMode: "HTML");
        }
        catch (System.Exception ex)
        {
            _ = MongoLogService.LogErrorAsync(chatId, "ZabbixError", ex.Message, ex.StackTrace);
            await TelegramMessageSender.SendMessageAsync(AppSettingsService.TelegramToken, chatId, "Error while getting data from Zabbix: " + ex.Message);
        }
    }

    public async Task HandleHelp(long chatId, string cmd)
    {
        await TelegramMessageSender.SendMessageAsync(AppSettingsService.TelegramToken, chatId,
            "Available commands:\n• sw / swimming – pool water level\n• home / temp – home sensors from Zabbix\n• help – this message");
    }
}
