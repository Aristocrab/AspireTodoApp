using AspireTodoApp.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireTodoApp.ApiService.Database;

public class AppDbContext : DbContext
{
    public DbSet<TodoTask> Tasks { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}