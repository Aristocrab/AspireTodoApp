using AspireTodoApp.ApiService.Database;
using AspireTodoApp.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Error = ErrorOr.Error;

namespace AspireTodoApp.ApiService.Services;

public class PostgresTasksService : ITasksService
{
    private readonly AppDbContext _context;

    public PostgresTasksService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TodoTask>> GetAllTasks()
    {
        return await _context.Tasks.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<ErrorOr<TodoTask>> GetTaskById(Guid id)
    {
        if (id == Guid.Empty)
        {
            return Error.Validation("Invalid task ID.");
        }
        
        var result = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        
        if (result is null)
        {
            return Error.NotFound("Task not found.");
        }
        
        return result;
    }

    public async Task<ErrorOr<Created>> AddTask(CreateTodoTaskDto createTodoTaskDto)
    {
        if (string.IsNullOrWhiteSpace(createTodoTaskDto.Title))
        {
            return Error.Validation("Title cannot be empty.");
        }
        
        var task = new TodoTask(createTodoTaskDto.Id ?? Guid.NewGuid(), createTodoTaskDto.Title, createTodoTaskDto.Description,
            Status.Pending, DateTime.UtcNow);

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> ToggleTaskStatus(Guid taskId)
    {
        var existing = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (existing is null)
        {
            return Error.NotFound("Task not found.");
        }

        existing.Status = existing.Status == Status.Pending ? Status.Completed : Status.Pending;
        await _context.SaveChangesAsync();
        
        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> UpdateTask(UpdateTodoTaskDto updateTodoTaskDto)
    {
        if (string.IsNullOrWhiteSpace(updateTodoTaskDto.Title))
        {
            return Error.Validation("Title cannot be empty.");
        }
        
        var existing = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == updateTodoTaskDto.Id);
        if (existing is null)
        {
            return Error.NotFound("Task not found.");
        }

        existing.Title = updateTodoTaskDto.Title;
        existing.Description = updateTodoTaskDto.Description;
        await _context.SaveChangesAsync();
        
        return Result.Updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteTask(Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task is  null)
        {
            return Error.NotFound("Task not found.");
        }
        
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return Result.Deleted;
    }
}