using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

public static class WaterLevelHistoryRepository
/// <summary>
/// Persists water level estimations to MongoDB for history.
/// </summary>
{
    private static IMongoCollection<BsonDocument> _history;
    private static bool _initialized = false;

    public static void Init(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(dbName);
        _history = db.GetCollection<BsonDocument>("WaterLevelHistory");
        _initialized = true;
    }

    public static async Task AddRecordAsync(long userId, int percent, string source = "telegram")
    {
        if (!_initialized) throw new Exception("WaterLevelHistoryRepository is not initialized");

        var doc = new BsonDocument
        {
            { "userId", userId },
            { "timestamp", DateTime.UtcNow },
            { "levelPercent", percent },
            { "source", source }
        };

        await _history.InsertOneAsync(doc);
    }
}
