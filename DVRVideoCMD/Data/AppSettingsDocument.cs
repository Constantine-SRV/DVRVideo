using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;

/// <summary>Документ с настройками, который хранится в MongoDB (коллекция "settings").</summary>
public class AppSettingsDocumentOld_dell
{
    [BsonId]
    public ObjectId Id { get; set; }

    // ---- Zabbix ----
    public string ZabbixApiUrl { get; set; }
    public string ZabbixUsername { get; set; }
    public string ZabbixPassword { get; set; }
    public string ZabbixApiToken { get; set; }

    // ---- Камеры / Telegram ----
    public string BaseRtspUrl { get; set; }
    public string TelegramToken { get; set; }

    // ---- Azure OpenAI ----
    public string AzureOpenAiKey { get; set; }
    public string AzureOpenAiEndpoint { get; set; }

    // ---- Списки ----
    public List<SnapshotArea> SnapshotAreas { get; set; } = new();

    // Храним словари как массивы { k: <key>, v: <value> }, чтобы поддерживались int/long-ключи
  //  [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
   // public Dictionary<long, UserInfo> Users { get; set; } = new();

    [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
    public Dictionary<int, string> CameraNames { get; set; } = new();
}
