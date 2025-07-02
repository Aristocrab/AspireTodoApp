using AspireTodoApp.ApiService.Models;
using AspireTodoApp.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireTodoApp.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITasksService _tasksService;

    public TasksController(ITasksService tasksService)
    {
        _tasksService = tasksService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TodoTask>>> GetAllTasks()
    {
        return await _tasksService.GetAllTasks();
    }

    [HttpGet("{taskId}")]
    public async Task<ActionResult<TodoTask>> GetTaskById(Guid taskId)
    {
        var result = await _tasksService.GetTaskById(taskId);
        return result.Match<ActionResult<TodoTask>>(
            task => Ok(task),
            errors => Problem(statusCode: 404, title: errors.FirstOrDefault().Code)
        );
    }

    [HttpPost("new")]
    public async Task<ActionResult> AddTask(CreateTodoTaskDto createTodoTaskDto)
    {
        var result = await _tasksService.AddTask(createTodoTaskDto);
        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => Problem(statusCode: 400, title: errors.FirstOrDefault().Code)
        );
    }

    [HttpPut("update")]
    public async Task<ActionResult> UpdateTask(UpdateTodoTaskDto updateTodoTaskDto)
    {
        var result = await _tasksService.UpdateTask(updateTodoTaskDto);
        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => Problem(statusCode: 400, title: errors.FirstOrDefault().Code)
        );
    }

    [HttpPut("toggle/{taskId}")]
    public async Task<ActionResult> ToggleTaskStatus(Guid taskId)
    {
        var result = await _tasksService.ToggleTaskStatus(taskId);
        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => Problem(statusCode: 404, title: errors.FirstOrDefault().Code)
        );
    }

    [HttpDelete("delete")]
    public async Task<ActionResult> DeleteTask(Guid taskId)
    {
        var result = await _tasksService.DeleteTask(taskId);
        return result.Match<ActionResult>(
            _ => NoContent(),
            errors => Problem(statusCode: 404, title: errors.FirstOrDefault().Code)
        );
    }
}
