using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class TelegramUpdateListener
/// <summary>
/// Polls Telegram for new messages and triggers the command handler delegate.
/// Used by TelegramBotBackgroundService.
/// </summary>
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
            var updatesUrl = $"{_apiUrl}getUpdates?timeout=60&offset={_offset}";
            var resp = await _http.GetStringAsync(updatesUrl);

            using var doc = JsonDocument.Parse(resp);
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
    }
}
