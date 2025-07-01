using AspireTodoApp.ApiService.Database;
using AspireTodoApp.ApiService.Models;
using HotChocolate.Data;

namespace AspireTodoApp.ApiService.GraphQL;

public class Query
{
    [UseSorting]
    [UseFiltering]
    public IExecutable<TodoTask> GetTodoTasks([Service] MongoDbContext context) =>
        context.TodoTasks.AsExecutable();
}