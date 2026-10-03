using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Leaderboard.Tests.FunctionalTests.Helpers;

internal static class TokenHelper
{
    public const string Issuer = "TestIssuer";
    public const string Audience = "TestAudience";
    public const string SigningKey = "functional-tests-signing-key-at-least-32-bytes";

    public static string GetToken(long ownerId = 1)
    {
        return GetToken([new Claim(JwtRegisteredClaimNames.Sub, ownerId.ToString())]);
    }

    public static string GetToken(IEnumerable<Claim> claims, string signingKey = SigningKey,
        DateTime? expiresAt = null)
    {
        var expires = expiresAt ?? DateTime.UtcNow.AddMinutes(15);

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            NotBefore = expires.AddMinutes(-30),
            Expires = expires,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                SecurityAlgorithms.HmacSha256)
        });
    }
}