using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Uses Azure OpenAI to estimate pool water level from two camera images.
/// Called by TelegramCommandProcessor when handling the "swim" command.
/// </summary>
public class WaterLevelAiAnalyzer
{
    private readonly string _endpoint;
    private readonly string _deployment;
    private readonly string _apiKey;
    private readonly string _apiVersion;

    public WaterLevelAiAnalyzer(string endpoint, string deployment, string apiKey, string apiVersion = "2024-02-15-preview")
    {
        _endpoint = endpoint;
        _deployment = deployment;
        _apiKey = apiKey;
        _apiVersion = apiVersion;
    }

    public async Task<string> AnalyzeWaterLevelAsync(string imagePath1, string imagePath2,long chatId)
    {
        string prompt = @"On these two photos of the pool, estimate the water level as a percentage of the height of the rectangular skimmer window (the opening in the wall). Only reply with the number (percentage), no extra text.
Additional information: These are two photos of the same pool taken from different cameras, placed at different angles. One camera is mounted at a height of 2.8 meters above the water surface, the other at 6 meters. Please account for possible optical distortions and refraction at the water surface when making your estimate.";

        var imageBytes1 = Convert.ToBase64String(File.ReadAllBytes(imagePath1));
        var imageBytes2 = Convert.ToBase64String(File.ReadAllBytes(imagePath2));

        var requestBody = new
        {
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{imageBytes1}" } },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{imageBytes2}" } }
                    }
                }
            },
            max_tokens = 100,
            temperature = 0.2,
            top_p = 0.95,
        };

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

        var url = $"{_endpoint}openai/deployments/{_deployment}/chat/completions?api-version={_apiVersion}";
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var choice = choices[0];
                if (choice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var contentProp))
                {
                    int percentInt;
                    if (int.TryParse(contentProp.GetString(), out percentInt))
                    {
                        WaterLevelHistoryService.AddRecordAsync(chatId, percentInt);
                    }
                    return $"{contentProp.GetString()}% of the skimmer height (estimated by {_deployment})";

                }
            }

            // В ответе нет ожидаемых полей — логируем ошибку
            Console.WriteLine("[ERROR] Unexpected response from OpenAI:");
            Console.WriteLine(responseContent);
            _ = MongoLogService.LogErrorAsync(0, "OpenAIBadResponse", responseContent);
            return $"AI response error {responseContent}";
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Exception while parsing OpenAI response:");
            Console.WriteLine(responseContent);
            _ = MongoLogService.LogErrorAsync(0, "OpenAIParseException", ex.Message + "\n" + responseContent, ex.StackTrace);
            return "AI parsing error";
        }
    }
}
