using Forest.Contracts;
using Forest.Domain.Entities;
using Forest.Domain.Exceptions;
using Forest.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI configuration:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Forest API", Version = "v1" });
});

// DB:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Initialize the database and apply migrations:
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
}

// TODO: Move to a script file and run separately
// Seed the database with some initial data (for test purposes):
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!await db.Nodes.AnyAsync())
    {
        var root = new Node("Root");
        var child1 = new Node("Child 1", root.Id);
        var child2 = new Node("Child 2", root.Id);
        var grandchild1 = new Node("Grandchild 1", child1.Id);
        db.Nodes.AddRange(root, child1, child2, grandchild1);
        await db.SaveChangesAsync();
    }
}

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ex is null) return;

        context.Response.ContentType = "application/json";

        var (status, title) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, ex.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
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

app.MapGet("/api/nodes/{name}", async (String name, AppDbContext db, CancellationToken ct) =>
{
    var node = await db.Nodes.AsNoTracking()
        .Where(x => x.Name == name)
        .Select(x => new { x.Id, x.Name, x.ParentId, x.CreatedAt })
        .SingleOrDefaultAsync(ct);
    return node is not null ? Results.Ok(node) : throw new KeyNotFoundException($"Node \"{name}\" does not exist");
})
.Produces<NodeDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/nodes", async (CreateNodeRequest req, AppDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        throw new ValidationException("Name is required.");

    await using var tx = await db.Database.BeginTransactionAsync(ct);

    if (req.ParentId is not null) {
        var parentExists = await db.Nodes.AnyAsync(n => n.Id == req.ParentId.Value, ct);
        if (!parentExists) throw new KeyNotFoundException("Parent node not found.");
    }

    var node = new Node(req.Name.Trim(), req.ParentId);
    db.Nodes.Add(node);

    await db.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);

    var dto = new NodeDto(node.Id, node.Name, node.ParentId);
    return Results.Created($"/api/nodes/{node.Id}", dto);
})
.Produces<NodeDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

app.Run();
