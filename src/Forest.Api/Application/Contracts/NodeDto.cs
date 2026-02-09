namespace Forest.Application.Contracts;

public record NodeDto (Guid Id, string Name, Guid? ParentId);

public record NodeDetailsDto (Guid Guid, string Name, Guid? ParentId, DateTimeOffset CreatedAt);
