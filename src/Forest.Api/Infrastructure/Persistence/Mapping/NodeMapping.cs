using Forest.Domain.Entities;
using Forest.Infrastructure.Persistence.Entities;

namespace Forest.Infrastructure.Persistence.Mapping;

public static class NodeMapping
{
    public static Node ToDomain(this NodeEntity e) => new Node(e.Id, e.Name, e.ParentId);

    public static void ApplyDomain(this NodeEntity e, Node d)
    {
        e.Name = d.Name;
        e.ParentId = d.ParentId;
    }
}
