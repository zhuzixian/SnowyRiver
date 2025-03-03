using System.Security.Claims;

namespace SnowyRiver.Jwt;
public interface ITokenService
{
    string BuildToken(IEnumerable<Claim> claims, JwtOptions options);
}
