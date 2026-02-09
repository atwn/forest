namespace Forest.Application.Contracts;

public record CreateNodeRequest(string Name, Guid? ParentId);
