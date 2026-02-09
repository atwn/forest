using Forest.Application.Abstractions;
using Forest.Application.Contracts;
using Forest.Application.Services;
using Forest.Domain.Exceptions;
using Forest.Infrastructure.Auth;
using Forest.Infrastructure.Persistence;
using Forest.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI setup:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Forest API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Use '/auth/login' endpoint to get a token"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var cfg = builder.Configuration;
builder.Services.Configure<JwtOptions>(cfg.GetSection("Jwt"));

// DI:
builder.Services.AddScoped<IUnitOfWork, EFUnitOfWork>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<HierarchyService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<AuthService>();

// DB:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// AuthN:
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromSeconds(30) // ?
                };
            });

// AuthZ:
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

var app = builder.Build();

// Swagger:
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// Initialize the database and apply migrations:
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
}

app.MapPost("/auth/login", (LoginRequest req, AuthService auth, IConfiguration cfg) =>
{
    var minutes = int.TryParse(cfg["Jwt:LifetimeMinutes"], out var lm) ? lm : 15;
    var lifetime = TimeSpan.FromMinutes(minutes);

    var (token, expires) = auth.Login(req.Username, req.Password, DateTime.UtcNow, lifetime);
    return Results.Ok(new TokenResponse(token, expires));
})
.AllowAnonymous();

app.MapGet("api/nodes/search", async (string? name, HierarchyService svc, CancellationToken ct) =>
{
    var nodes = await svc.SearchAsync(name ?? string.Empty, ct);
    return Results.Ok(nodes);
})
.RequireAuthorization()
.Produces<List<NodeDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.MapGet("/api/nodes/{id}", async (Guid id, HierarchyService svc, CancellationToken ct) =>
{
    var body = await svc.GetAsync(id, ct);
    return Results.Ok(body);
})
.RequireAuthorization()
.Produces<NodeDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/nodes", async (CreateNodeRequest req, HierarchyService svc, CancellationToken ct) =>
{
    var node = await svc.CreateAsync(req, ct);
    return Results.Created($"/api/nodes/{node.Id}", node);
})
.RequireAuthorization("AdminOnly")
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
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ex.Message),
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
