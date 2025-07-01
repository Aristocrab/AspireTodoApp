namespace AspireTodoApp.ApiService.Models;

public record CreateTodoTaskDto(string Title, string? Description, Status Status);