using AspireTodoApp.ApiService.Models;
using AspireTodoApp.ApiService.Services;

namespace AspireTodoApp.ApiService.Database;

public class DbSeeder
{
    private readonly ITasksService _tasksService;
    private readonly AppDbContext _dbContext;

    public DbSeeder(ITasksService tasksService, AppDbContext dbContext)
    {
        _tasksService = tasksService;
        _dbContext = dbContext;
    }

    public async Task SeedDatabaseAsync()
    {
        // Ensure the database is created
        await _dbContext.Database.EnsureCreatedAsync();
        
        var tasks = await _tasksService.GetAllTasks();
        if (tasks.Count > 0)
        {
            return; // Database already seeded
        }

        // Seed with some initial tasks
        var initialTasks = new List<CreateTodoTaskDto>
        {
            new("Buy groceries", "Milk, Bread, Eggs"),
            new("Walk the dog"),
            new("Finish project report", "Due next week"),
            new("Call mom"),
            new("Prepare for meeting", "Review agenda and documents")
        };

        foreach (var task in initialTasks)
        {
            await _tasksService.AddTask(task);
        }
    }
}