using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api1/values")]
public class ValuesController : ControllerBase
{
    [HttpGet("sensor")]
    public IActionResult Sensor()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        Console.WriteLine($"--- Values/sensor | {DateTime.Now:yyyy-MM-dd HH:mm:ss} | IP: {ip} ---");
        foreach (var kv in Request.Query)
            Console.WriteLine($"{kv.Key} = {kv.Value}");

        Console.WriteLine("RAW: " + Request.QueryString.Value);
        return Ok("OK");
    }
}
