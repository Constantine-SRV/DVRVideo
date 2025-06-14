using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api1/hotWater")]
/// <summary>
/// HTTP endpoint receiving hot water sensor values.
/// Data is accumulated and later forwarded to Zabbix.
/// </summary>
public class HotWaterSensorController : ControllerBase
{
    private readonly SensorDataAccumulator _accumulator;
    public HotWaterSensorController(SensorDataAccumulator accumulator)
    {
        _accumulator = accumulator;
    }

    [HttpGet("in1")]
    public IActionResult In1()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        string host = "homeCY";

        // локальный helper: парсим float безопасно
        double? F(string s)
            => double.TryParse(s?.Replace(',', '.'),
                               System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture,
                               out var v) ? v : (double?)null;

        double? amp = F(Request.Query["a"]);
        double? t1 = F(Request.Query["t1"]);
        double? t2 = F(Request.Query["t2"]);
        double? t3 = F(Request.Query["t3"]);

        // ► отправляем в аккуму- или сразу в Zabbix
        void Add(string key, double? v)
        {
            if (v.HasValue)
                _accumulator.Add(host, key, v.Value);   // или _zabbix.SendAsync(...)
        }

        Add("amp.3f.wh", amp);
        Add("temp.3f.hw1", t1);
        Add("temp.3f.hw2", t2);
        Add("temp.3f.p1", t3);

        // ---------- ОТВЕТ ДЛЯ ESP ---------------------------------
        // Формируем строку   amp|t1|t2|t3   (если чего-то нет — «0.00»)
        string fmt(double? v) => (v ?? 0).ToString("0.00",
                                   System.Globalization.CultureInfo.InvariantCulture);

        string response = $"{fmt(amp)}|{fmt(t1)}|{fmt(t2)}|{fmt(t3)}";

        // Вернём plain-text без кавычек
        return Content(response, "text/plain");
    }

}
