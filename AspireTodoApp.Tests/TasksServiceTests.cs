using AspireTodoApp.ApiService.Database;
using AspireTodoApp.ApiService.Models;
using AspireTodoApp.ApiService.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AspireTodoApp.Tests;

public class TasksServiceTests
{
    private readonly ITasksService _tasksService;
    private readonly AppDbContext _dbContext;
    
    public TasksServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nameof(TasksServiceTests))
            .Options;

        _dbContext = new AppDbContext(options);

        _dbContext.Database.EnsureDeleted();
        
        var tasks = new List<TodoTask>
        {
            new(Guid.NewGuid(), "Buy groceries", "Milk, Bread, Eggs", Status.Pending, DateTime.UtcNow),
            new(Guid.NewGuid(), "Walk the dog", null, Status.Pending, DateTime.UtcNow),
            new(Guid.NewGuid(), "Finish project report", "Due next week", Status.Pending, DateTime.UtcNow),
            new(Guid.NewGuid(), "Call mom", null, Status.Pending, DateTime.UtcNow),
            new(Guid.NewGuid(), "Prepare for meeting", "Review agenda and documents", Status.Pending, DateTime.UtcNow)
        };
        _dbContext.Tasks.AddRange(tasks);
        _dbContext.SaveChanges();
        
        _tasksService = new PostgresTasksService(_dbContext);
    }

    [Fact]
    public async Task GetAllTasks_should_return_all_tasks()
    {
        // Act
        var tasks = await _tasksService.GetAllTasks();
        
        // Assert
        tasks.Count.ShouldBe(5);
    }

    [Fact]
    public async Task GetTaskById_when_task_exists_should_return_task()
    {
        // Arrange
        var existingTask = await _dbContext.Tasks.FirstAsync(x => x.Title == "Buy groceries");
        
        // Act
        var task = await _tasksService.GetTaskById(existingTask.Id);
        
        // Assert
        task.IsError.ShouldBeFalse();
        task.Value.Id.ShouldBe(existingTask.Id);
        task.Value.Title.ShouldBe(existingTask.Title);
        task.Value.Description.ShouldBe(existingTask.Description);
        task.Value.Status.ShouldBe(existingTask.Status);
        task.Value.CreatedAt.ShouldBe(existingTask.CreatedAt);
    }
    
    [Fact]
    public async Task GetTaskById_when_task_id_is_empty_should_return_error()
    {
        // Arrange
        var id = Guid.Empty;
        
        // Act
        var task = await _tasksService.GetTaskById(id);
        
        // Assert
        task.IsError.ShouldBeTrue();
        task.Errors.Count.ShouldBeGreaterThan(0);
    }
    
    [Fact]
    public async Task GetTaskById_when_task_does_not_exist_should_return_error()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        var task = await _tasksService.GetTaskById(id);
        
        // Assert
        task.IsError.ShouldBeTrue();
        task.Errors.Count.ShouldBeGreaterThan(0);
    }
    
    [Fact]
    public async Task AddTask_with_valid_data_should_add_task()
    {
        // Arrange
        var createDto = new CreateTodoTaskDto("New task", "Description");
    
        // Act
        var result = await _tasksService.AddTask(createDto);
    
        // Assert
        result.IsError.ShouldBeFalse();
        var addedTask = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Title == "New task");
        addedTask.ShouldNotBeNull();
        addedTask.Title.ShouldBe(createDto.Title);
        addedTask.Description.ShouldBe(createDto.Description);
        addedTask.Status.ShouldBe(Status.Pending);
    }

    [Fact]
    public async Task AddTask_with_empty_title_should_return_error()
    {
        // Arrange
        var createDto = new CreateTodoTaskDto("");
    
        // Act
        var result = await _tasksService.AddTask(createDto);
    
        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ToggleTaskStatus_when_task_exists_should_toggle_status()
    {
        // Arrange
        var existingTask = await _dbContext.Tasks.FirstAsync(x => x.Title == "Walk the dog");
        var originalStatus = existingTask.Status;
    
        // Act
        var result = await _tasksService.ToggleTaskStatus(existingTask.Id);
    
        // Assert
        result.IsError.ShouldBeFalse();
        var updatedTask = await _dbContext.Tasks.FirstAsync(t => t.Id == existingTask.Id);
        updatedTask.Status.ShouldBe(originalStatus == Status.Pending ? Status.Completed : Status.Pending);
    }

    [Fact]
    public async Task ToggleTaskStatus_when_task_does_not_exist_should_return_error()
    {
        // Arrange
        var id = Guid.NewGuid();
    
        // Act
        var result = await _tasksService.ToggleTaskStatus(id);
    
        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateTask_with_valid_data_should_update_task()
    {
        // Arrange
        var existingTask = await _dbContext.Tasks.FirstAsync(x => x.Title == "Call mom");
        var updateDto = new UpdateTodoTaskDto(existingTask.Id, "Updated task title", "Updated description");
    
        // Act
        var result = await _tasksService.UpdateTask(updateDto);
    
        // Assert
        result.IsError.ShouldBeFalse();
        var updatedTask = await _dbContext.Tasks.FirstAsync(t => t.Id == existingTask.Id);
        updatedTask.Title.ShouldBe(updateDto.Title);
        updatedTask.Description.ShouldBe(updateDto.Description);
    }

    [Fact]
    public async Task UpdateTask_with_empty_title_should_return_error()
    {
        // Arrange
        var existingTask = await _dbContext.Tasks.FirstAsync(x => x.Title == "Buy groceries");
        var updateDto = new UpdateTodoTaskDto(existingTask.Id, "", "Description");
    
        // Act
        var result = await _tasksService.UpdateTask(updateDto);
    
        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateTask_when_task_does_not_exist_should_return_error()
    {
        // Arrange
        var updateDto = new UpdateTodoTaskDto(Guid.NewGuid(), "Non-existent task", "Description");
    
        // Act
        var result = await _tasksService.UpdateTask(updateDto);
    
        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task DeleteTask_when_task_exists_should_delete_task()
    {
        // Arrange
        var existingTask = await _dbContext.Tasks.FirstAsync(x => x.Title == "Finish project report");
    
        // Act
        var result = await _tasksService.DeleteTask(existingTask.Id);
    
        // Assert
        result.IsError.ShouldBeFalse();
        var deletedTask = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == existingTask.Id);
        deletedTask.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteTask_when_task_does_not_exist_should_return_error()
    {
        // Arrange
        var id = Guid.NewGuid();
    
        // Act
        var result = await _tasksService.DeleteTask(id);
    
        // Assert
        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }
}