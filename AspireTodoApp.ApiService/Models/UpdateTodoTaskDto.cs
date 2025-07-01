namespace AspireTodoApp.ApiService.Models;

public record UpdateTodoTaskDto(Guid Id, string Title, string? Description);