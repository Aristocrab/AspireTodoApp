using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AspireTodoApp.ApiService.Models;

[ValidateNever]
public record CreateTodoTaskDto(string Title, string? Description = null, Guid? Id = null);