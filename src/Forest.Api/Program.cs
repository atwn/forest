using Forest.Api.Domain.Entities;
using Forest.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Initialize the database:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize the database and apply migrations:
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
}

// TODO: Remove
// Seed the database with some initial data (for test purposes);
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

app.MapGet("/api/nodes/{name}", async (String name, AppDbContext db) =>
{
    var node = await db.Nodes.AsNoTracking()
        .Where(x => x.Name == name)
        .Select(x => new { x.Id, x.Name, x.ParentId, x.CreatedAt })
        .SingleOrDefaultAsync();
    return node is not null ? Results.Ok(node) : Results.NotFound();
})
.WithName("Get Node by Name")
.WithOpenApi();

app.Run();
