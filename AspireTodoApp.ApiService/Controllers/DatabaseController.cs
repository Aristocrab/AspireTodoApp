using AspireTodoApp.ApiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspireTodoApp.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    [HttpGet("database")]
    public ActionResult<string> GetCurrentDatabase()
    {
        return SyncedTasksService.Database;
    }

    [HttpPut("toggle/{database}")]
    public Task<ActionResult> ToggleDatabase(string database)
    {
        if (database.ToLower() != "postgres" && database.ToLower() != "mongo")
        {
            return Task.FromResult<ActionResult>(
                BadRequest("Invalid database specified. Use 'Postgres' or 'Mongo'."));
        }

        SyncedTasksService.Database = database;
        return Task.FromResult<ActionResult>(NoContent());
    }
}