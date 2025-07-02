using AspireTodoApp.ApiService.Database;
using AspireTodoApp.ApiService.Models;
using AspireTodoApp.ApiService.Services;
using HotChocolate.Data;
using MongoDB.Driver;

namespace AspireTodoApp.ApiService.GraphQL;

public class Query
{
    [UseSorting]
    [UseFiltering]
    public IExecutable<TodoTask> GetTodoTasks(
        [Service] MongoDbContext mongoDbContext,
        [Service] AppDbContext postgresDbContext)
    {
        if (SyncedTasksService.Database == "postgres")
        {
            return postgresDbContext.Tasks.OrderByDescending(x => x.CreatedAt).AsExecutable();
        }

        return mongoDbContext.TodoTasks
            .Find("{}")
            .Sort(Builders<TodoTask>.Sort.Descending(x => x.CreatedAt))
            .AsExecutable();
    }
}