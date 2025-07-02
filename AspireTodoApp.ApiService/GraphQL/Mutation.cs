using AspireTodoApp.ApiService.Models;
using AspireTodoApp.ApiService.Services;

namespace AspireTodoApp.ApiService.GraphQL;

public class Mutation
{
    public async Task<bool> AddTodoTask(string title, string? description, [Service] ITasksService tasksService)
    {
        var result = await tasksService.AddTask(new CreateTodoTaskDto(title, description));
        
        return !result.IsError;
    }
    
    public async Task<bool> ToggleTaskStatus(Guid taskId, [Service] ITasksService tasksService)
    {
        var result = await tasksService.ToggleTaskStatus(taskId);
        
        return !result.IsError;
    }

    public async Task<bool> UpdateTask(Guid id, string title, string? description, [Service] ITasksService tasksService)
    {
        var updateDto = new UpdateTodoTaskDto(id, title, description);
        var result = await tasksService.UpdateTask(updateDto);
        
        return !result.IsError;
    }

    public async Task<bool> DeleteTask(Guid taskId, [Service] ITasksService tasksService)
    {
        var result = await tasksService.DeleteTask(taskId);
        
        return !result.IsError;
    }
}
