using AspireTodoApp.ApiService.Models;
using ErrorOr;

namespace AspireTodoApp.ApiService.Services;

public class SyncedTasksService : ITasksService
{
    public static string Database = "postgres";
    
    private readonly PostgresTasksService _postgresTasksService;
    private readonly MongoTasksService _mongoTasksService;

    public SyncedTasksService(PostgresTasksService postgresTasksService, MongoTasksService mongoTasksService)
    {
        _postgresTasksService = postgresTasksService;
        _mongoTasksService = mongoTasksService;
    }

    public async Task<List<TodoTask>> GetAllTasks()
    {
        if(Database == "postgres")
        {
            return await _postgresTasksService.GetAllTasks();
        }
        
        return await _mongoTasksService.GetAllTasks();
    }

    public async Task<ErrorOr<TodoTask>> GetTaskById(Guid id)
    {
        if (Database == "postgres")
        {
            return await _postgresTasksService.GetTaskById(id);
        }
        
        return await _mongoTasksService.GetTaskById(id);
    }

    public async Task<ErrorOr<Created>> AddTask(CreateTodoTaskDto createTodoTaskDto)
    {
        var dto = createTodoTaskDto with
        {
            Id = Guid.NewGuid()
        };
        
        var postgresResult = await _postgresTasksService.AddTask(dto);
        var mongoResult = await _mongoTasksService.AddTask(dto);

        if (Database == "postgres")
        {
            return postgresResult;
        }
        
        return mongoResult;
    }

    public async Task<ErrorOr<Updated>> ToggleTaskStatus(Guid taskId)
    {
        var postgresResult = await _postgresTasksService.ToggleTaskStatus(taskId);
        var mongoResult = await _mongoTasksService.ToggleTaskStatus(taskId);
        
        if (Database == "postgres")
        {
            return postgresResult;
        }
        
        return mongoResult;
    }

    public async Task<ErrorOr<Updated>> UpdateTask(UpdateTodoTaskDto updateTodoTaskDto)
    {
        var postgresResult = await _postgresTasksService.UpdateTask(updateTodoTaskDto);
        var mongoResult = await _mongoTasksService.UpdateTask(updateTodoTaskDto);
        
        if (Database == "postgres")
        {
            return postgresResult;
        }
        
        return mongoResult;
    }

    public async Task<ErrorOr<Deleted>> DeleteTask(Guid taskId)
    {
        var postgresResult = await _postgresTasksService.DeleteTask(taskId);
        var mongoResult = await _mongoTasksService.DeleteTask(taskId);
        
        if (Database == "postgres")
        {
            return postgresResult;
        }
        
        return mongoResult;
    }
}