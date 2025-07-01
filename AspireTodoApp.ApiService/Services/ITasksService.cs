using AspireTodoApp.ApiService.Models;
using ErrorOr;

namespace AspireTodoApp.ApiService.Services;

public interface ITasksService
{
    Task<List<TodoTask>> GetAllTasks();
    Task<ErrorOr<TodoTask>> GetTaskById(Guid id);
    Task<ErrorOr<Created>> AddTask(CreateTodoTaskDto createTodoTaskDto);
    Task<ErrorOr<Updated>> ToggleTaskStatus(Guid taskId);
    Task<ErrorOr<Updated>> UpdateTask(UpdateTodoTaskDto updateTodoTaskDto);
    Task<ErrorOr<Deleted>> DeleteTask(Guid taskId);
}