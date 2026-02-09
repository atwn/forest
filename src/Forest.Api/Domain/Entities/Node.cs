using Forest.Domain.Exceptions;

namespace Forest.Domain.Entities;

public sealed class Node
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public Guid? ParentId { get; private set; }

    public Node(Guid id, string name, Guid? parentId = null)
    {
        Id = id;
        Rename(name);
        ParentId = parentId;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Name is required");
        }

        Name = name.Trim();
    }
}
