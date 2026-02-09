using Forest.Domain.Exceptions;

namespace Forest.Domain.Entities;

public sealed class Node
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public Guid? ParentId { get; private set; }
    public Node? Parent { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;


    private Node() { }

    public Node(string name, Guid? parentId = null)
    {
        this.UpdateName(name);
        ParentId = parentId;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Name is required");
        }

        Name = name.Trim();
    }
}
