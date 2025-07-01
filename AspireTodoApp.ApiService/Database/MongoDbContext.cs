using AspireTodoApp.ApiService.Models;
using MongoDB.Driver;

namespace AspireTodoApp.ApiService.Database;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client)
    {
        _database = client.GetDatabase("mongodb");
    }

    public IMongoCollection<TodoTask> TodoTasks => _database.GetCollection<TodoTask>("tasks");
}
