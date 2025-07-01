namespace AspireTodoApp.ApiService.Models;

public record TodoTask(Guid Id, string Title, string? Description, Status Status, DateTime CreatedAt);