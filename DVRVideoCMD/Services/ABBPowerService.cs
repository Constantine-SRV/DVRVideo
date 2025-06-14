using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;


public class ABBPowerService
{

    public static async Task<PowerValues> GetPowerValuesAsync()
    {
        try
        {
            using var hc = new HttpClient();
            hc.Timeout = TimeSpan.FromSeconds(2);

            string username = AppSettingsService.ABBuser;
            string password = AppSettingsService.ABBPassword;
            string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"));
            hc.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encoded);

            string url = AppSettingsService.ABBurl;
            string resp = await hc.GetStringAsync(url);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var root = JsonSerializer.Deserialize<Root>(resp, options);

            var result = new PowerValues();
            var points = root?._1004543P251820?.points;

            if (points == null)
                return new PowerValues(); // все значения по умолчанию (0)

            foreach (var p in points)
            {
                if (p.name == "Pin1") result.Pin1 = p.value;
                if (p.name == "Pin2") result.Pin2 = p.value;
                if (p.name == "Pgrid") result.Pgrid = p.value;
                if (p.name == "Temp1") result.Temp1 = p.value;
                if (p.name == "E0_runtime") result.E0_runtime = p.value;
                if (p.name == "Fgrid") result.Fgrid = p.value;
            }

            return result;
        }
        catch (Exception ex)
        {
            // Возвращаем PowerValues с нулями при любой ошибке
            if (DateTime.Now.Hour > 7 && DateTime.Now.Hour < 20)
            {
                Console.WriteLine($"ABBPowerService{ex.Message}");
            }
            return null;
        }
    }
}

    public class PowerValues
{
    public double Pin1 { get; set; }
    public double Pin2 { get; set; }
    public double Pgrid { get; set; }
    public double Temp1 { get; set; }
    public double E0_runtime { get; set; }
    public double Fgrid { get; set; }
   // public double HouseP1 { get; set; }
  //  public double HouseP2 { get; set; }
  //  public double HouseP3 { get; set; }
  //  public DateTime dt { get; set; }
}

public class Point
{
    public string name { get; set; }
    public double value { get; set; }
}

public class _1004543P251820
{
    public string device_id { get; set; }
    public string device_type { get; set; }
    public string device_model { get; set; }
    public string timestamp { get; set; }
    public List<Point> points { get; set; }
}

public class Root
{
    [JsonPropertyName("100454-3P25-1820")] //serial number of an inverter
    public _1004543P251820 _1004543P251820 { get; set; }
}
