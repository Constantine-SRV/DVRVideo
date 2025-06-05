using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;

public class ZabbixApiService
{
    private readonly string _url;
    private readonly string _user;
    private readonly string _password;
    private readonly string _apiToken; // если используешь токен (6.0+)

    public ZabbixApiService(string url, string user, string password, string apiToken = null)
    {
        _url = url;
        _user = user;
        _password = password;
        _apiToken = apiToken;
    }

    private async Task<string> LoginAsync()
    {
        if (!string.IsNullOrEmpty(_apiToken))
            return _apiToken; // Zabbix 6+, просто подставляем токен

        var data = new
        {
            jsonrpc = "2.0",
            method = "user.login",
            @params = new { user = _user, password = _password },
            id = 1
        };

        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        var resp = await client.PostAsync(_url, content);
        var respStr = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(respStr);
        return doc.RootElement.GetProperty("result").GetString();
    }



    public async Task<List<ItemLastValue>> GetLastValuesAsync(string host)
    {
        var auth = await LoginAsync();

        // Получаем hostid по имени
        var hostReq = new
        {
            jsonrpc = "2.0",
            method = "host.get",
            @params = new
            {
                output = new[] { "hostid", "host" },
                filter = new { host = new[] { host } }
            },
            auth = auth,
            id = 2
        };

        using var client = new HttpClient();
        if (!string.IsNullOrEmpty(_apiToken))
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiToken);

        var hostReqContent = new StringContent(JsonSerializer.Serialize(hostReq), Encoding.UTF8, "application/json");
        var hostResp = await client.PostAsync(_url, hostReqContent);
        var hostJson = await hostResp.Content.ReadAsStringAsync();
        var hostDoc = JsonDocument.Parse(hostJson);
        var hostId = hostDoc.RootElement.GetProperty("result")[0].GetProperty("hostid").GetString();

        // Получаем все items по hostid
        var itemsReq = new
        {
            jsonrpc = "2.0",
            method = "item.get",
            @params = new
            {
                output = "extend",
                hostids = new[] { hostId }
            },
            auth = auth,
            id = 3
        };
        var itemsReqContent = new StringContent(JsonSerializer.Serialize(itemsReq), Encoding.UTF8, "application/json");
        var itemsResp = await client.PostAsync(_url, itemsReqContent);
        var itemsJson = await itemsResp.Content.ReadAsStringAsync();
        var itemsDoc = JsonDocument.Parse(itemsJson);

        var result = new List<ItemLastValue>();

        foreach (var item in itemsDoc.RootElement.GetProperty("result").EnumerateArray())
        {
            var name = item.GetProperty("name").GetString();
            var lastvalue = item.GetProperty("lastvalue").GetString();
            var prevvalue = item.GetProperty("prevvalue").GetString();
            var lastclock = item.GetProperty("lastclock").GetString();

            string time = "";
            if (long.TryParse(lastclock, out var unixTime))
            {
                time = DateTimeOffset.FromUnixTimeSeconds(unixTime)
                        .ToLocalTime()
                        .ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }

            // Форматирование до 2 знаков после запятой (если число, иначе оставляем оригинал)
            string lastVal2 = lastvalue;
            if (double.TryParse(lastvalue, NumberStyles.Any, CultureInfo.InvariantCulture, out var vLast))
                lastVal2 = vLast.ToString("F2", CultureInfo.InvariantCulture);

            string prevVal2 = prevvalue;
            if (double.TryParse(prevvalue, NumberStyles.Any, CultureInfo.InvariantCulture, out var vPrev))
                prevVal2 = vPrev.ToString("F2", CultureInfo.InvariantCulture);

            result.Add(new ItemLastValue
            {
                Name = name,
                LastValue = lastVal2,
                PrevValue = prevVal2,
                Time = time
            });
        }


        return result;
    }

}


public class ItemLastValue
{
    public string Name { get; set; }
    public string LastValue { get; set; }
    public string PrevValue { get; set; }
    public string Time { get; set; }
}