namespace Forest.Contracts;

public record CreateNodeRequest(string Name, Guid? ParentId);
