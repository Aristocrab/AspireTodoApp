namespace AspireTodoApp.ApiService.Models;

public class TodoTask
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public TodoTask(Guid id, string title, string? description, Status status, DateTime createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        CreatedAt = createdAt;
    }
}
