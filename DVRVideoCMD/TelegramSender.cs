public static class TelegramSender
{
    public static async Task SendPhotoAsync(string token, long chatId, string imagePath, string caption = null)
    {
        using var client = new HttpClient();
        var url = $"https://api.telegram.org/bot{token}/sendPhoto";
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(chatId.ToString()), "chat_id");
        if (!string.IsNullOrEmpty(caption))
            form.Add(new StringContent(caption), "caption");
        form.Add(new StreamContent(File.OpenRead(imagePath)), "photo", Path.GetFileName(imagePath));

        var response = await client.PostAsync(url, form);
        string result = await response.Content.ReadAsStringAsync();

        // Логика — смотрим по ответу Telegram, был ли "ok":true
        if (response.IsSuccessStatusCode && result.Contains("\"ok\":true"))
            Console.WriteLine($"Sent photo: {imagePath}, Telegram response: ok");
        else
            Console.WriteLine($"Sent photo: {imagePath}, Telegram response: error");
    }

    public static async Task SendMessageAsync(string token, long chatId, string text, string parseMode = null)
    {
        using var client = new HttpClient();
        var url = $"https://api.telegram.org/bot{token}/sendMessage";
        var parameters = new Dictionary<string, string>
    {
        { "chat_id", chatId.ToString() },
        { "text", text }
    };
        if (!string.IsNullOrEmpty(parseMode))
            parameters.Add("parse_mode", parseMode);

        var response = await client.PostAsync(url, new FormUrlEncodedContent(parameters));
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode && result.Contains("\"ok\":true"))
            Console.WriteLine($"Sent message to {chatId}: ok");
        else
            Console.WriteLine($"Sent message to {chatId}: error");
    }

}
