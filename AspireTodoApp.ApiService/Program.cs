using System.Diagnostics;
using AspireTodoApp.ApiService.Database;
using AspireTodoApp.ApiService.GraphQL;
using AspireTodoApp.ApiService.Services;
using AspireTodoApp.ServiceDefaults;
using Microsoft.AspNetCore.Mvc;
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

// Postgres
builder.AddNpgsqlDbContext<AppDbContext>(connectionName: "postgresdb");
builder.Services.AddTransient<PostgresTasksService>();

// MongoDB
builder.AddMongoDBClient(connectionName: "mongodb");
builder.Services.AddTransient<MongoTasksService>();

// Synced Tasks Service
builder.Services.AddTransient<ITasksService, SyncedTasksService>();

// Redis
builder.AddRedisDistributedCache(connectionName: "cache");
builder.Services.Decorate<ITasksService, CachedTasksService>();

// Database Seeder
builder.Services.AddTransient<DbSeeder>();

// GraphQL
builder.Services.AddSingleton<MongoDbContext>();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddMongoDbFiltering()
    .AddMongoDbSorting();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbSeeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await dbSeeder.SeedDatabaseAsync();
}

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