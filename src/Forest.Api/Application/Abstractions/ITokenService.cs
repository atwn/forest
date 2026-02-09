using System.Security.Claims;

namespace Forest.Application.Abstractions;

public interface ITokenService
{
    string IssueToken(IEnumerable<Claim> claims, DateTime utcNow, TimeSpan lifetime);
}
