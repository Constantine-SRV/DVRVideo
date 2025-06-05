using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api1/hotWater")]
public class HotWaterController : ControllerBase
{
    private readonly AccumulatorService _accumulator;
    public HotWaterController(AccumulatorService accumulator)
    {
        _accumulator = accumulator;
    }

    [HttpGet("in1")]
    public IActionResult In1()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        //Console.WriteLine($"--- HotWater/in1 | {DateTime.Now:yyyy-MM-dd HH:mm:ss} | IP: {ip} ---");
        //foreach (var kv in Request.Query)
        //    Console.WriteLine($"{kv.Key} = {kv.Value}");
       // Console.WriteLine("HotWater/in1 RAW: " + Request.QueryString.Value);

        string host = "homeCY";

        void AddIfFloat(string param, string value)
        {
            if (double.TryParse(value?.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                _accumulator.Add(host, param, v);
        }

        AddIfFloat("amp.3f.wh", Request.Query["a"]);
        AddIfFloat("temp.3f.hw1", Request.Query["t1"]);
        AddIfFloat("temp.3f.hw2", Request.Query["t2"]);
        AddIfFloat("temp.3f.p1", Request.Query["t3"]);

        return Ok("OK");
    }
}
