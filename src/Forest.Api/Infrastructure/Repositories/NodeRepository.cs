using Forest.Application.Abstractions;
using Forest.Domain.Entities;
using Forest.Infrastructure.Persistence;
using Forest.Infrastructure.Persistence.Entities;
using Forest.Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Forest.Infrastructure.Repositories;

public class NodeRepository : INodeRepository
{
    private readonly AppDbContext _db;
    public NodeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Node?> GetAsync(string name, CancellationToken ct)
    {
        var e = await _db.Nodes.AsNoTracking().SingleOrDefaultAsync(x => x.Name == name, ct);
        return e?.ToDomain();
    }

    public async Task AddAsync(Node node, CancellationToken ct)
    {
        var e = new NodeEntity
        {
            Id = node.Id,
            Name = node.Name,
            ParentId = node.ParentId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.Nodes.Add(e);
    }

    public Task<bool> ExistsAsync(Guid value, CancellationToken ct)
    {
        return _db.Nodes.AsNoTracking().AnyAsync(x => x.Id == value, ct);
    }
}
