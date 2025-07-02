using System.Text.Json;
using AspireTodoApp.ApiService.Models;
using Microsoft.Extensions.Caching.Distributed;
using ErrorOr;

namespace AspireTodoApp.ApiService.Services;

public class CachedTasksService : ITasksService
{
    private readonly ITasksService _tasksService;
    private readonly IDistributedCache _cache;

    public CachedTasksService(ITasksService tasksService, IDistributedCache cache)
    {
        _tasksService = tasksService;
        _cache = cache;
    }
    
    public async Task<List<TodoTask>> GetAllTasks()
    {
        var cached = await _cache.GetStringAsync("tasks:all");

        if (string.IsNullOrEmpty(cached))
        {
            var tasks = await _tasksService.GetAllTasks();
            await _cache.SetStringAsync("tasks:all", JsonSerializer.Serialize(tasks));

            return tasks;
        }

        return JsonSerializer.Deserialize<List<TodoTask>>(cached)!;
    }

    public async Task<ErrorOr<TodoTask>> GetTaskById(Guid id)
    {
        var cached = await _cache.GetStringAsync($"tasks:{id}");

        if (string.IsNullOrEmpty(cached))
        {
            var tasks = await _tasksService.GetTaskById(id);
            if (tasks.IsError)
            {
                return tasks.Errors;
            }
            await _cache.SetStringAsync($"tasks:{id}", JsonSerializer.Serialize(tasks.Value));

            return tasks;
        }

        return JsonSerializer.Deserialize<TodoTask>(cached)!;
    }

    public async Task<ErrorOr<Created>> AddTask(CreateTodoTaskDto createTodoTaskDto)
    {
        var result = await _tasksService.AddTask(createTodoTaskDto);
        await _cache.RemoveAsync("tasks:all");

        return result;
    }

    public async Task<ErrorOr<Updated>> ToggleTaskStatus(Guid taskId)
    {
        var result = await _tasksService.ToggleTaskStatus(taskId);
        await _cache.RemoveAsync("tasks:all");
        await _cache.RemoveAsync($"tasks:{taskId}");

        return result;
    }

    public async Task<ErrorOr<Updated>> UpdateTask(UpdateTodoTaskDto updateTodoTaskDto)
    {
        var result = await _tasksService.UpdateTask(updateTodoTaskDto);
        await _cache.RemoveAsync("tasks:all");
        await _cache.RemoveAsync($"tasks:{updateTodoTaskDto.Id}");

        return result;
    }

    public async Task<ErrorOr<Deleted>> DeleteTask(Guid taskId)
    {
        var result = await _tasksService.DeleteTask(taskId);
        await _cache.RemoveAsync("tasks:all");
        await _cache.RemoveAsync($"tasks:{taskId}");

        return result;
    }
}