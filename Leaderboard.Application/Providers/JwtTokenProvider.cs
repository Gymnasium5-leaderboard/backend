using System.Security.Claims;
using System.Text;
using Leaderboard.Domain.Entities;
using Leaderboard.Domain.Interfaces.Provider;
using Leaderboard.Domain.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Leaderboard.Application.Providers;

public class JwtTokenProvider(IOptions<JwtSettings> jwtSettings) : ITokenProvider
{
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly JwtSettings _settings = jwtSettings.Value;

    public (string Token, DateTime ExpiresAt) CreateAccessToken(LeaderboardOwner owner)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenLifetimeMinutes);

        var token = _tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiresAt,
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, owner.Id.ToString())]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey)),
                SecurityAlgorithms.HmacSha256)
        });

        return (token, expiresAt);
    }
}