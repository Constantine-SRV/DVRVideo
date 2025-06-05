using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Фасад для доступа к настройкам, хранящимся в MongoDB.</summary>
public static class AppSettingsService
{
    // ───────── публичные свойства ─────────
    public static string MongoConnectionString { get; private set; }
    public static string MongoDbName { get; private set; }

    // Zabbix
    public static string ZabbixApiUrl => _doc.ZabbixApiUrl;
    public static string ZabbixUsername => _doc.ZabbixUsername;
    public static string ZabbixPassword => _doc.ZabbixPassword;
    public static string ZabbixApiToken => _doc.ZabbixApiToken;

    // DVR / Telegram
    public static string BaseRtspUrl => _doc.BaseRtspUrl;
    public static string TelegramToken => _doc.TelegramToken;

    // Azure OpenAI
    public static string AzureOpenAiKey => _doc.AzureOpenAiKey;
    public static string AzureOpenAiEndpoint => _doc.AzureOpenAiEndpoint;

    // ABB (новые параметры)
    public static string ABBuser => _doc.ABBuser;
    public static string ABBPassword => _doc.ABBPassword;
    public static string ABBurl => _doc.ABBurl;

    // Разные списки
    public static List<SnapshotArea> SnapshotAreas => _doc.SnapshotAreas;
    public static Dictionary<int, string> CameraNames => _doc.CameraNames;

    public static string GetCameraName(int ch) =>
        CameraNames.TryGetValue(ch, out var n) ? n : $"ch_{ch:D2}";

    // ───────── загрузка ─────────
    public static void Load()
    {
        InitConnFromEnv();

        var client = new MongoClient(MongoConnectionString);
        var db = client.GetDatabase(MongoDbName);
        var col = db.GetCollection<SettingsDoc>("settings");

        _doc = col.Find(FilterDefinition<SettingsDoc>.Empty).FirstOrDefault()
               ?? throw new Exception("Settings document not found in MongoDB");
    }

    // ───────── private ─────────
    private static SettingsDoc _doc;

    private static void InitConnFromEnv()
    {
        MongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONN_STR")
                               ?? throw new InvalidOperationException("MONGO_CONN_STR not set");
        var url = new MongoUrl(MongoConnectionString);
        MongoDbName = string.IsNullOrEmpty(url.DatabaseName) ? "TgDB" : url.DatabaseName;
    }

    /// <summary>Приватный класс-контейнер для сериализации.</summary>
    private class SettingsDoc
    {
        [BsonId] public ObjectId Id { get; set; }

        // Zabbix
        public string ZabbixApiUrl { get; set; }
        public string ZabbixUsername { get; set; }
        public string ZabbixPassword { get; set; }
        public string ZabbixApiToken { get; set; }

        // DVR / Telegram
        public string BaseRtspUrl { get; set; }
        public string TelegramToken { get; set; }

        // Azure OpenAI
        public string AzureOpenAiKey { get; set; }
        public string AzureOpenAiEndpoint { get; set; }

        // --- новые ABB-параметры ---
        public string ABBuser { get; set; }
        public string ABBPassword { get; set; }
        public string ABBurl { get; set; }

        // Lists / dictionaries
        public List<SnapshotArea> SnapshotAreas { get; set; } = new();

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, string> CameraNames { get; set; } = new();
    }
}
