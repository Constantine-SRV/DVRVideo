using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

AppSettingsService.Load(); // <--- не забудь вызвать до всего, чтобы все параметры были проинициализированы!
MongoLogService.Init(AppSettingsService.MongoConnectionString, AppSettingsService.MongoDbName);
UserRegistry.Init(AppSettingsService.MongoConnectionString, AppSettingsService.MongoDbName);

WaterLevelHistoryRepository.Init(AppSettingsService.MongoConnectionString, AppSettingsService.MongoDbName);


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning); // <-- только Warning и выше

// Регистрируем singleton'ы для DI
builder.Services.AddSingleton<SensorDataAccumulator>();
builder.Services.AddSingleton<ZabbixSenderService>(sp =>
    new ZabbixSenderService("192.168.55.41")
);
// Пример DI для ZabbixApiClient — параметры берём из AppSettings:
builder.Services.AddSingleton<ZabbixApiClient>(sp =>
    new ZabbixApiClient(
        AppSettingsService.ZabbixApiUrl,
        AppSettingsService.ZabbixUsername,
        AppSettingsService.ZabbixPassword,
        AppSettingsService.ZabbixApiToken
    )
);

builder.Services.AddSingleton<WaterLevelAiAnalyzer>(sp =>
    new WaterLevelAiAnalyzer(
        AppSettingsService.AzureOpenAiEndpoint,
        "gpt-4o-mini",
        AppSettingsService.AzureOpenAiKey
    )
);


builder.Services.AddSingleton<TelegramCommandProcessor>();
builder.Services.AddSingleton<TelegramCommandRouter>();
builder.Services.AddHostedService<TelegramBotBackgroundService>();

builder.Services.AddControllers();

// Настройки веб-сервера — порт 80
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(80));


var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

// Middleware для красивого 404 — добавляй после всех эндпоинтов!
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var url = $"{context.Request.Path}{context.Request.QueryString}";
        Console.WriteLine($"--- 404 NotFound | {DateTime.Now:yyyy-MM-dd HH:mm:ss} | IP: {ip} ---");
        Console.WriteLine($"URL: {url}");
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("404 Not Found - request was logged");
    }
});

app.Run();
