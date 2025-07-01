using AspireTodoApp.ApiService.Database;
using AspireTodoApp.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using ErrorOr;
using Error = ErrorOr.Error;

namespace AspireTodoApp.ApiService.Services;

public class TasksService : ITasksService
{
    private readonly AppDbContext _context;

    public TasksService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TodoTask>> GetAllTasks()
    {
        return await _context.Tasks.ToListAsync();
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
        
        if (createTodoTaskDto.Status != Status.Pending && createTodoTaskDto.Status != Status.Completed)
        {
            return Error.Validation("Invalid status.");
        }
        
        var task = new TodoTask(Guid.NewGuid(), createTodoTaskDto.Title, createTodoTaskDto.Description,
            createTodoTaskDto.Status, DateTime.UtcNow);

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return Result.Created;
    }

    public async Task<ErrorOr<Updated>> ToggleTaskStatus(Guid taskId)
    {
        var existing = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId);
        if (existing is null)
        {
            return Error.NotFound("Task not found.");
        }

        var updated = existing with { Status = existing.Status == Status.Completed ? Status.Pending : Status.Completed };
        _context.Tasks.Update(updated);
        await _context.SaveChangesAsync();
        
        return Result.Updated;
    }

    public async Task<ErrorOr<Updated>> UpdateTask(UpdateTodoTaskDto updateTodoTaskDto)
    {
        var existing = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == updateTodoTaskDto.Id);
        if (existing is null)
        {
            return Error.NotFound("Task not found.");
        }

        var updated = existing with
        {
            Title = updateTodoTaskDto.Title,
            Description = updateTodoTaskDto.Description
        };

        _context.Tasks.Update(updated);
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