using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class ZabbixSenderService
{
    private readonly string _server;
    private readonly int _port;

    public ZabbixSenderService(string server, int port = 10051)
    {
        _server = server;
        _port = port;
    }

    public async Task SendAsync(string host, string key, string value)
    {
        // Протокол Zabbix trapper: [ZBXD\x01][data length][JSON data]
        var data = new
        {
            request = "sender data",
            data = new[]
            {
                new { host, key, value }
            }
        };
        var json = JsonSerializer.Serialize(data);
        var jsonBytes = Encoding.UTF8.GetBytes(json);

        // Формируем header
        byte[] header = Encoding.ASCII.GetBytes("ZBXD\x01");
        byte[] length = BitConverter.GetBytes((long)jsonBytes.Length);

        // Zabbix требует little-endian для длины
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(length);

        byte[] packet = new byte[header.Length + 8 + jsonBytes.Length];
        Buffer.BlockCopy(header, 0, packet, 0, header.Length);
        Buffer.BlockCopy(length, 0, packet, header.Length, 8);
        Buffer.BlockCopy(jsonBytes, 0, packet, header.Length + 8, jsonBytes.Length);

        using (var client = new TcpClient())
        {
            await client.ConnectAsync(_server, _port);
            using var stream = client.GetStream();
            await stream.WriteAsync(packet, 0, packet.Length);

            // Ответ от Zabbix можно прочитать, если нужно (но обычно не нужен для fire-and-forget)
            byte[] response = new byte[1024];
            int read = await stream.ReadAsync(response, 0, response.Length);
            string resp = Encoding.UTF8.GetString(response, 0, read);
            //Console.WriteLine("Zabbix response: " + resp);
        }
    }
}
