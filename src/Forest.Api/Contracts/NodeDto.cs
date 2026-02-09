namespace Forest.Contracts;

public record NodeDto (Guid Id, string Name, Guid? ParentId);
