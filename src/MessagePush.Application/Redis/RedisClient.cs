using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace MessagePush.Redis;

public class RedisClient
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _database;

    public RedisClient(IConfiguration configuration)
    {
        var redisConfiguration = configuration.GetSection("Redis:Configuration").Value;
        _connection = ConnectionMultiplexer.Connect(redisConfiguration);
        _database = _connection.GetDatabase();
    }
    
    public void Set(string key, int value)
    {
        _database.StringSet(key, value);
    }

    public int Get(string key)
    {
        var value = _database.StringGet(key);
        return (int)value;
    }
    
    public int IncrementAndGet(string key)
    {
        return (int) _database.StringIncrement(key);
    }
    
    public int GetAndIncrement(string key)
    {
        return (int) _database.StringIncrement(key) - 1;
    }
    
    public bool AddIfNotExists(string key, int value)
    {
        return _database.StringSet(key, value, when: When.NotExists);
    }
}