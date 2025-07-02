using AspireTodoApp.ApiService.Models;
using ErrorOr;
using MongoDB.Driver;
using Error = ErrorOr.Error;

namespace AspireTodoApp.ApiService.Services;

public class MongoTasksService : ITasksService
{
    private readonly IMongoClient _client;

    public MongoTasksService(IMongoClient client)
    {
        _client = client;
    }
    
    public async Task<List<TodoTask>> GetAllTasks()
    {
        var db = _client.GetDatabase("mongodb");

        var tasks = await db.GetCollection<TodoTask>("tasks")
            .Find("{}")
            .Sort(Builders<TodoTask>.Sort.Descending(x => x.CreatedAt))
            .ToListAsync();

        return tasks;
    }

    public async Task<ErrorOr<TodoTask>> GetTaskById(Guid id)
    {
        var db = _client.GetDatabase("mongodb");
        
        var task = await db.GetCollection<TodoTask>("tasks")
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();

        return task;
    }

    public async Task<ErrorOr<Created>> AddTask(CreateTodoTaskDto createTodoTaskDto)
    {
        if (string.IsNullOrWhiteSpace(createTodoTaskDto.Title))
        {
            return Error.Validation("Title cannot be empty.");
        }
        
        var db = _client.GetDatabase("mongodb");

        var task = new TodoTask(createTodoTaskDto.Id ?? Guid.NewGuid(), createTodoTaskDto.Title, createTodoTaskDto.Description,
            Status.Pending, DateTime.Now);

        await db.GetCollection<TodoTask>("tasks")
            .InsertOneAsync(task);
        
        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> ToggleTaskStatus(Guid taskId)
    {
        var db = _client.GetDatabase("mongodb");
        
        var collection = db.GetCollection<TodoTask>("tasks");

        var task = await collection.Find(x => x.Id == taskId).FirstOrDefaultAsync();
        if (task == null)
        {
            return Error.NotFound("Task not found.");
        }

        var newStatus = task.Status == Status.Completed 
            ? Status.Pending 
            : Status.Completed;

        var update = Builders<TodoTask>.Update.Set(x => x.Status, newStatus);
        await collection.UpdateOneAsync(x => x.Id == taskId, update);
        
        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> UpdateTask(UpdateTodoTaskDto updateTodoTaskDto)
    {
        if (string.IsNullOrWhiteSpace(updateTodoTaskDto.Title))
        {
            return Error.Validation("Title cannot be empty.");
        }
        
        var db = _client.GetDatabase("mongodb");

        var update = Builders<TodoTask>.Update
            .Set(x => x.Title, updateTodoTaskDto.Title)
            .Set(x => x.Description, updateTodoTaskDto.Description);
        
        await db.GetCollection<TodoTask>("tasks")
            .UpdateOneAsync(x => x.Id == updateTodoTaskDto.Id, update);
        
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteTask(Guid taskId)
    {
        var db = _client.GetDatabase("mongodb");

        await db.GetCollection<TodoTask>("tasks")
            .DeleteOneAsync(x => x.Id == taskId);
        
        return Result.Deleted;
    }
}