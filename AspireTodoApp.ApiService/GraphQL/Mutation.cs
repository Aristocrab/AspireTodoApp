using AspireTodoApp.ApiService.Database;
using AspireTodoApp.ApiService.Models;
using MongoDB.Driver;

namespace AspireTodoApp.ApiService.GraphQL;

public class Mutation
{
    public async Task<TodoTask> AddTodoTask(string title, string? description, [Service] MongoDbContext context)
    {
        var task = new TodoTask(Guid.NewGuid(), title, description, Status.Pending, DateTime.UtcNow);
        await context.TodoTasks.InsertOneAsync(task);
        return task;
    }
    
    public async Task<bool> ToggleTaskStatus(Guid taskId, [Service] MongoDbContext context)
    {
        var task = await context.TodoTasks
            .Find(x => x.Id == taskId)
            .FirstOrDefaultAsync();

        if (task == null)
            return false;

        var newStatus = task.Status == Status.Completed ? Status.Pending : Status.Completed;

        var update = Builders<TodoTask>.Update.Set(x => x.Status, newStatus);

        var result = await context.TodoTasks.UpdateOneAsync(x => x.Id == taskId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateTask(Guid id, string title, string? description, [Service] MongoDbContext context)
    {
        var update = Builders<TodoTask>.Update
            .Set(x => x.Title, title)
            .Set(x => x.Description, description);

        var result = await context.TodoTasks.UpdateOneAsync(x => x.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteTask(Guid taskId, [Service] MongoDbContext context)
    {
        var result = await context.TodoTasks.DeleteOneAsync(x => x.Id == taskId);
        return result.DeletedCount > 0;
    }
}
