using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api1/zabbix")]
/// <summary>
/// Proxy controller exposing a small portion of the Zabbix API.
/// </summary>
public class ZabbixController : ControllerBase
{
    private readonly ZabbixApiClient _zabbix;
    public ZabbixController(ZabbixApiClient zabbix) => _zabbix = zabbix;

    [HttpGet("lastvalues/{host}")]
    public async Task<IActionResult> GetLastValues(string host)
        => Ok(await _zabbix.GetLastValuesAsync(host));
}
