using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api1/power")]
/// <summary>
/// HTTP endpoint for power sensor readings.
/// Data is averaged and pushed to Zabbix.
/// </summary>
public class PowerSensorController : ControllerBase
{
    private readonly SensorDataAccumulator _accumulator;

    public PowerSensorController(SensorDataAccumulator accumulator)
    {
        _accumulator = accumulator;
    }

    [HttpGet("in1")]
    public IActionResult In1()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
       // Console.WriteLine("Power/in1 RAW: " + Request.QueryString.Value);

        string host = "homeCY";

        void AddIfFloat(string param, string value)
        {
            if (double.TryParse(value?.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                _accumulator.Add(host, param, v);
        }

        // Параметры и соответствие их item'ам в Zabbix:
        AddIfFloat("temp.3f.office.1", Request.Query["t"]);
        AddIfFloat("humidity.3f.office.1", Request.Query["hi"]);
        // если понадобятся еще — добавь аналогично

        return Ok("OK");
    }
}


/*
 * --- Power/in1 | 2025-06-01 16:47:30 | IP: ::ffff:192.168.0.102 ---
a = 0.40
t = 33.00 item temp.3f.office.1
h = 1.00 
hi = 29.4 humidity.3f.office.1
time = 0
key = office-power-1212
RAW: ?a=0.40&t=33.00&h=1.00&hi=29.4&time=0&key=office-power-1212


*/
