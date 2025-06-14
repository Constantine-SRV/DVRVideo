using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Globalization;

[ApiController]
[Route("api1/rs485Power")]
public class Rs485PowerController : ControllerBase
{
    private readonly ZabbixSenderService _zabbix;
    private const string Host = "homeCY";
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public Rs485PowerController(ZabbixSenderService zabbix) => _zabbix = zabbix;

    // ─────────────────────────────────────────────────────────────
    //  Минутные значения от Arduino  (/PowerCurrent)
    // ─────────────────────────────────────────────────────────────
    [HttpGet("PowerCurrent")]
    public IActionResult PowerCurrent()
    {
        var q = Request.Query;
        var dev = q["device"].ToString();
        if (string.IsNullOrEmpty(dev))
            return BadRequest("device missing");

        // собираем все float-параметры в словарь
        var vals = new Dictionary<string, double>();

        foreach (var kv in q)
        {
            if (kv.Key == "device") continue;
            if (double.TryParse(kv.Value.ToString().Replace(',', '.'),
                                NumberStyles.Float, Inv, out var v))
                vals[kv.Key] = v;
        }

        if (vals.Count == 0) return Ok("no data");

        // 1) отправляем «как есть» в Zabbix
        foreach (var kv in vals)
        {
            double val = kv.Value;

            // единственная коррекция — Solar p2p → абсолютное
            if (dev == "SDM72_Main" && kv.Key == "p2p")
                val = Math.Abs(val);

            Send($"{dev}.{kv.Key}", val);
        }

        // 2) считаем CurentPower, если все три p-парам есть
        if (vals.TryGetValue("p1p", out var p1) &&
            vals.TryGetValue("p2p", out var p2) &&
            vals.TryGetValue("p3p", out var p3))
        {
            double cur =
                dev switch
                {
                    "SDM72_Main" => p1 + p3 + p2,
                    "SDM72_2F" => p1 + p2 + p3,
                    _ => double.NaN
                };

            if (!double.IsNaN(cur))
                Send($"{dev}.CurentPower", cur);
        }

        return Ok("OK");

        void Send(string key, double val) =>
            _ = _zabbix.SendAsync(Host, key, val.ToString(Inv)); // fire-and-forget
    }

    // ─────────────────────────────────────────────────────────────
    //  Почасовой crie/cree  (/PowerTotal)  — пока только логируем
    // ─────────────────────────────────────────────────────────────
    [HttpGet("PowerTotal")]
    public IActionResult PowerTotal()
    {
        Console.WriteLine($"[PowerTotal] {DateTime.Now:yyyy-MM-dd HH:mm:ss} "
                        + Request.QueryString.Value);
        // здесь позже будет запись в MongoDB
        return Ok("logged");
    }
}
