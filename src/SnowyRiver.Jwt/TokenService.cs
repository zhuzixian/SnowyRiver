using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SnowyRiver.Jwt;
public class TokenService : ITokenService
{
    public string BuildToken(IEnumerable<Claim> claims, JwtOptions options)
    {
        var expiryDuration = TimeSpan.FromSeconds(options.ExpireSeconds);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(options.Issuer, options.Audience, claims,
            expires: DateTime.Now.Add(expiryDuration), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
