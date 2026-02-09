using Forest.Application.Abstractions;
using System.Security.Claims;

namespace Forest.Application.Services;

public class AuthService
{
    private readonly ITokenService _tokenService;

    // NB! Use a proper user store and password hashing
    private static readonly (string u, string p, string role)[] Users =
    [
        ("admin", "admin", "Admin"),
        ("user", "user", "Reader"),
    ];

    public AuthService(ITokenService tokenService) => _tokenService = tokenService;

    public (string token, int expiresInSeconds) Login(
        string username,
        string password,
        DateTime utcNow,
        TimeSpan lifetime)
    {
        var user = Users.SingleOrDefault(x =>
            string.Equals(x.u, username, StringComparison.Ordinal) && 
            x.p == password);

        if (user == default)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.u),
            new Claim(ClaimTypes.Role, user.role),
        };

        var token = _tokenService.IssueToken(claims, utcNow, lifetime);
        return (token, (int)lifetime.TotalSeconds);
    }
}
