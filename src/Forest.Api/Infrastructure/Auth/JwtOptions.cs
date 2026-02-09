namespace Forest.Infrastructure.Auth;

public class JwtOptions
{
    public string Issuer { get; init; } = "ForestOwner";
    public string Audience { get; init; } = "ForestUsers";
    public string Key { get; init; } = "<to-be-changed-must-be-at-least-32chars-long>";
    public int LifetimeMinutes { get; init; } = 15;
}
