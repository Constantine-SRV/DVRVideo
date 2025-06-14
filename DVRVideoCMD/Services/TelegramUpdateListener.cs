using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class TelegramUpdateListener
{
    private readonly string _token;
    private readonly string _apiUrl;
    private readonly HttpClient _http;
    private int _offset = 0;

    public delegate Task CommandHandler(long chatId, string text, int accessLevel);
    public event CommandHandler _commandHandler;

    public TelegramUpdateListener(string token)
    {
        _token = token;
        _apiUrl = $"https://api.telegram.org/bot{_token}/";
        _http = new HttpClient();
    }

    public async Task StartListeningAsync()
    {
        Console.WriteLine("[INFO] Bot started listening for Telegram updates...");
        while (true)
        {
            try
            {
                var updatesUrl = $"{_apiUrl}getUpdates?timeout=60&offset={_offset}";

                // Получаем апдейты от Telegram с обработкой ошибок запроса
                string resp;
                try
                {
                    var httpResponse = await _http.GetAsync(updatesUrl);
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[WARN] Telegram API returned {httpResponse.StatusCode}. Retrying in 10s...");
                        await Task.Delay(10_000);
                        continue;
                    }
                    resp = await httpResponse.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] Telegram HTTP request failed: " + ex.Message);
                    await Task.Delay(10_000);
                    continue;
                }

                // Парсим ответ Telegram
                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(resp);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] Failed to parse Telegram API response: " + ex.Message);
                    Console.WriteLine(resp);
                    await MongoLogService.LogErrorAsync(0, "TelegramListener", ex.Message, ex.StackTrace);

                    await Task.Delay(10_000);
                    continue;
                }

                var root = doc.RootElement;
                if (!root.TryGetProperty("result", out var results) || results.GetArrayLength() == 0)
                {
                    await Task.Delay(1000);
                    continue;
                }

                foreach (var update in results.EnumerateArray())
                {
                    _offset = update.GetProperty("update_id").GetInt32() + 1;
                    if (!update.TryGetProperty("message", out var msg)) continue;
                    if (!msg.TryGetProperty("chat", out var chat)) continue;
                    long chatId = chat.GetProperty("id").GetInt64();

                    string username = chat.TryGetProperty("username", out var unameProp) ? unameProp.GetString() ?? "" : "";
                    string firstName = chat.TryGetProperty("first_name", out var fnameProp) ? fnameProp.GetString() ?? "" : "";
                    string lastName = chat.TryGetProperty("last_name", out var lnameProp) ? lnameProp.GetString() ?? "" : "";
                    string language = chat.TryGetProperty("language_code", out var langProp) ? langProp.GetString() ?? "" : "";

                    if (!msg.TryGetProperty("text", out var textProp)) continue;
                    string text = textProp.GetString()?.ToLowerInvariant() ?? "";

                    // Регистрируем или обновляем пользователя — получаем accessLevel
                    int accessLevel = await UserRegistry.RegisterOrUpdateUserAsync(
                        chatId, username, firstName, lastName, language);

                    Console.WriteLine($"[INFO] Message from user {chatId} ({username}, access: {accessLevel}): {text}");

                    if (_commandHandler != null)
                        await _commandHandler.Invoke(chatId, text, accessLevel);
                }
            }
            catch (Exception ex)
            {
                // Любая НЕобработанная ошибка в цикле
                Console.WriteLine("[CRITICAL] Telegram update listener error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                await MongoLogService.LogErrorAsync(0, "TelegramListener", ex.Message, ex.StackTrace);

                // Ждем и пробуем дальше
                await Task.Delay(15_000);
            }
        }
    }
}
