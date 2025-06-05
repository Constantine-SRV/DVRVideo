using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api1/zabbix")]
public class ZabbixController : ControllerBase
{
    private readonly ZabbixApiService _zabbix;
    public ZabbixController(ZabbixApiService zabbix) => _zabbix = zabbix;

    [HttpGet("lastvalues/{host}")]
    public async Task<IActionResult> GetLastValues(string host)
        => Ok(await _zabbix.GetLastValuesAsync(host));
}
