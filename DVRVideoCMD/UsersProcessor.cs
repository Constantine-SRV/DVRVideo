using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class UsersProcessor
{
    private static IMongoCollection<BsonDocument> _users;
    private static HashSet<long> _knownUsers = new HashSet<long>();
    private static bool _initialized = false;

    public static void Init(string mongoConnectionString, string dbName)
    {
        var client = new MongoClient(mongoConnectionString);
        var db = client.GetDatabase(dbName);
        _users = db.GetCollection<BsonDocument>("Users");

        // Загружаем всех существующих пользователей в память
        var allUserIds = _users.Find(new BsonDocument()).Project(Builders<BsonDocument>.Projection.Include("userId")).ToList();
        foreach (var doc in allUserIds)
        {
            if (doc.Contains("userId")) _knownUsers.Add(doc["userId"].AsInt64);
        }
        _initialized = true;
        Console.WriteLine($"UsersProcessor initialized. Users loaded: {_knownUsers.Count}");
    }

    /// <summary>
    /// Проверяет, новый ли это пользователь, и если новый — добавляет в БД и память.
    /// Всегда обновляет lastSeen и прочие поля.
    /// </summary>
    public static async Task<int> RegisterOrUpdateUserAsync(long userId, string username, string firstName, string lastName, string language, int accessLevel = 0)
    {
        if (!_initialized) throw new Exception("UsersProcessor not initialized");

        var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);

        var update = Builders<BsonDocument>.Update
            .SetOnInsert("createdAt", DateTime.UtcNow)
            .SetOnInsert("accessLevel", accessLevel)
            .Set("username", username)
            .Set("first_name", firstName)
            .Set("last_name", lastName)
            .Set("language", language)
            .Set("lastSeen", DateTime.UtcNow);

        var opts = new UpdateOptions { IsUpsert = true };
        await _users.UpdateOneAsync(filter, update, opts);

        if (_knownUsers.Add(userId))
            Console.WriteLine($"New user registered: {userId} {username}");

        // Получаем accessLevel (добавили проверку на наличие поля)
        var userDoc = await _users.Find(filter).FirstOrDefaultAsync();
        if (userDoc != null && userDoc.Contains("accessLevel"))
            return userDoc["accessLevel"].AsInt32;
        return 0;
    }

    /// <summary>
    /// Проверяет, есть ли пользователь в памяти.
    /// </summary>
    public static bool IsKnownUser(long userId) => _knownUsers.Contains(userId);

    /// <summary>
    /// Можно использовать для получения всех userId (например, для рассылок).
    /// </summary>
    public static IEnumerable<long> GetAllUserIds() => _knownUsers;
}
