using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api1/hotWaterYUN")]
/// <summary>
/// HTTP endpoint receiving hot water YUN sensor values (2F).
/// </summary>
public class HotWaterYunController : ControllerBase
{
    private readonly SensorDataAccumulator _accumulator;
    public HotWaterYunController(SensorDataAccumulator accumulator)
    {
        _accumulator = accumulator;
    }

    [HttpGet("in1")]
    public IActionResult In1()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        string host = "homeCY";

        // helper for float parsing
        double? F(string s)
            => double.TryParse(s?.Replace(',', '.'),
                               System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture,
                               out var v) ? v : (double?)null;

        double? t1 = F(Request.Query["t1"]);
        double? t2 = F(Request.Query["t2"]);
        double? t3 = F(Request.Query["t3"]);

        // Записываем значения (amp нет, только 2F температуры)
        void Add(string key, double? v)
        {
            if (v.HasValue)
                _accumulator.Add(host, key, v.Value);
        }

        Add("temp.2f.hw1", t1);
        Add("temp.2f.hw2", t2);
        Add("temp.2f.p1", t3);

        // Формируем строку ответа
        string fmt(double? v) => (v ?? 0).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        string response = $"{fmt(t1)}|{fmt(t2)}|{fmt(t3)}";

        return Content(response, "text/plain");
    }
}
