using System.Collections.Concurrent;
using System.Timers;

public class AccumulatorService
{
    public class ValueBucket
    {
        public double Sum = 0;
        public int Count = 0;
        public void Add(double v) { Sum += v; Count++; }
        public double Average => Count == 0 ? 0 : Sum / Count;
        public void Reset() { Sum = 0; Count = 0; }
    }

    // Хранилище: host -> param -> bucket
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ValueBucket>> _buckets = new();

    private readonly ZabbixSenderService _zabbix;
    private readonly System.Timers.Timer _timer;

    public AccumulatorService(ZabbixSenderService zabbix)
    {
        _zabbix = zabbix;
        _timer = new System.Timers.Timer(60_000); // 60 сек
        _timer.Elapsed += (s, e) => FlushAll();
        _timer.AutoReset = true;
        _timer.Start();
    }

    public void Add(string host, string param, double value)
    {
        var hostDict = _buckets.GetOrAdd(host, _ => new ConcurrentDictionary<string, ValueBucket>());
        var bucket = hostDict.GetOrAdd(param, _ => new ValueBucket());
        bucket.Add(value);
    }

    public void FlushAll()
    {
        foreach (var hostKvp in _buckets)
        {
            var host = hostKvp.Key;
            foreach (var paramKvp in hostKvp.Value)
            {
                var param = paramKvp.Key;
                var bucket = paramKvp.Value;
                if (bucket.Count > 0)
                {
                    var avg = bucket.Average.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    _ = _zabbix.SendAsync(host, param, avg); // fire-and-forget
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sent avg: host={host}, key={param}, avg={avg}, count={bucket.Count}");
                    bucket.Reset();
                }
            }
        }
    }
}
