using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public static class MongoLogService
{
    private static IMongoCollection<BsonDocument> _logs;
    private static IMongoCollection<BsonDocument> _errors;
    private static bool _initialized = false;

    // Инициализация только один раз
    public static void Init(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);
        _logs = db.GetCollection<BsonDocument>("Logs");
        _errors = db.GetCollection<BsonDocument>("Errors");
        _initialized = true;
    }

    public static async Task LogAsync(long userId, string eventName, string info = null)
    {
        if (!_initialized) throw new Exception("MongoLogService is not initialized");

        var doc = new BsonDocument
        {
            { "userId", userId },
            { "timestamp", DateTime.UtcNow },
            { "event", eventName }
        };
        if (!string.IsNullOrEmpty(info))
            doc["info"] = info;

        await _logs.InsertOneAsync(doc);
    }

    public static async Task LogErrorAsync(long userId, string errorType, string message, string stackTrace = null)
    {
        if (!_initialized) throw new Exception("MongoLogService is not initialized");

        var doc = new BsonDocument
        {
            { "userId", userId },
            { "timestamp", DateTime.UtcNow },
            { "errorType", errorType },
            { "message", message }
        };
        if (!string.IsNullOrEmpty(stackTrace))
            doc["stackTrace"] = stackTrace;

        await _errors.InsertOneAsync(doc);
    }
}
