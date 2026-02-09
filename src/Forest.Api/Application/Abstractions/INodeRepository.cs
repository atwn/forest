using Forest.Domain.Entities;

namespace Forest.Application.Abstractions;

public interface INodeRepository
{
    Task<Node?> GetAsync(string name, CancellationToken ct);

    Task AddAsync(Node node, CancellationToken ct);

    Task<bool> ExistsAsync(Guid value, CancellationToken ct);
}
