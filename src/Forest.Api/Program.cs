using Forest.Application.Abstractions;
using Forest.Application.Contracts;
using Forest.Application.Services;
using Forest.Domain.Exceptions;
using Forest.Infrastructure.Persistence;
using Forest.Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI configuration:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Forest API", Version = "v1" });
});

// DI:
builder.Services.AddScoped<IUnitOfWork, EFUnitOfWork>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<HierarchyService>();

// DB:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Swagger:
app.UseSwagger();
app.UseSwaggerUI();

// Initialize the database and apply migrations:
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
}

app.MapGet("api/nodes/search", async (string? name, HierarchyService svc, CancellationToken ct) =>
{
    var nodes = await svc.SearchAsync(name ?? string.Empty, ct);
    return Results.Ok(nodes);
})
.Produces<List<NodeDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.MapGet("/api/nodes/{id}", async (Guid id, HierarchyService svc, CancellationToken ct) =>
{
    var body = await svc.GetAsync(id, ct);
    return Results.Ok(body);
})
.Produces<NodeDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/nodes", async (CreateNodeRequest req, HierarchyService svc, CancellationToken ct) =>
{
    var node = await svc.CreateAsync(req, ct);
    return Results.Created($"/api/nodes/{node.Id}", node);
})
.Produces<NodeDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

// Global error handling:
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ex is null) return;

        context.Response.ContentType = "application/json";

        var (status, title) = ex switch
        {
            DomainException => (StatusCodes.Status400BadRequest, ex.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error.")
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new
        {
            title,
            status,
        });
    });
});

app.Run();
