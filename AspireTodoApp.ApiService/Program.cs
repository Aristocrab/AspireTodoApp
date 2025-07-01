using System.Diagnostics;
using AspireTodoApp.ApiService.Database;
using AspireTodoApp.ApiService.GraphQL;
using AspireTodoApp.ApiService.Services;
using AspireTodoApp.ServiceDefaults;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.OpenTelemetry()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog();

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Postgres:
builder.AddNpgsqlDbContext<AppDbContext>(connectionName: "postgresdb");
builder.Services.AddTransient<ITasksService, TasksService>();

// MongoDB
builder.AddMongoDBClient(connectionName: "mongodb");
builder.Services.AddTransient<ITasksService, MongoTasksService>();

// Redis
builder.AddRedisDistributedCache(connectionName: "cache");
builder.Services.Decorate<ITasksService, CachedTasksService>();

// GraphQL
builder.Services.AddSingleton<MongoDbContext>();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddMongoDbFiltering()
    .AddMongoDbSorting();

builder.Services.AddControllers();

var app = builder.Build();

app.MapGraphQL();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.Map("/", () => Results.Redirect("/scalar"));
}

app.Use(async (context, next) =>
{
    app.Logger.LogInformation("{RequestMethod} {RequestPath} started", 
        context.Request.Method, 
        context.Request.Path);
            
    var stopwatch = new Stopwatch();
            
    stopwatch.Start();
    await next(context);
    stopwatch.Stop();
            
    app.Logger.LogInformation("{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms",
        context.Request.Method, 
        context.Request.Path, 
        context.Response.StatusCode, 
        stopwatch.Elapsed.TotalMilliseconds);
});

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();