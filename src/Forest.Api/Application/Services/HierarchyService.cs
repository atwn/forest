using Forest.Application.Abstractions;
using Forest.Application.Contracts;
using Forest.Domain.Entities;
using Forest.Domain.Exceptions;

namespace Forest.Application.Services;

public class HierarchyService
{
    private readonly INodeRepository _repo;
    private readonly IUnitOfWork _uow;

    public HierarchyService(INodeRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<NodeDto> GetAsync(Guid id, CancellationToken ct)
    {
        var node = await _repo.GetAsync(id, ct);
        return node != null ? ToDto(node) : throw new KeyNotFoundException($"Node \"{id}\" does not exist");
    }

    public async Task<NodeDto> CreateAsync(CreateNodeRequest req, CancellationToken ct)
    {
        return await _uow.ExecuteInTransactionAsync(async ct =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new DomainException("Name is required.");

            if (req.ParentId is not null) {
                var parentExists = await _repo.ExistsAsync(req.ParentId.Value, ct);
                if (!parentExists) throw new KeyNotFoundException("Parent node not found.");
            }

            var node = new Node(Guid.NewGuid(), req.Name.Trim(), req.ParentId);
            await _repo.AddAsync(node, ct);

            return ToDto(node);
        }, ct);
    }

    public async Task<List<NodeDto>> SearchAsync(string name, CancellationToken ct)
    {
        return await _repo.SearchAsync(name, ct).ContinueWith(t => t.Result.Select(ToDto).ToList(), ct);
    }

    private static NodeDto ToDto(Node node) => new(node.Id, node.Name, node.ParentId);
}
