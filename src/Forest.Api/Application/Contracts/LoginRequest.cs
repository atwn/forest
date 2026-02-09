namespace Forest.Application.Contracts;

public sealed record LoginRequest(string Username, string Password);
public sealed record TokenResponse(string AccessToken, int ExpiresInSeconds);