namespace Forest.Infrastructure.Persistence.Entities;

public sealed class NodeEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid? ParentId { get; set; }

    public NodeEntity? Parent { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
