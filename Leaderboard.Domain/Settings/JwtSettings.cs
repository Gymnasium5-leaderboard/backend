namespace Leaderboard.Domain.Settings;

public class JwtSettings
{
    public string Issuer { get; set; } 
    public string Audience { get; set; } 
    public string SigningKey { get; set; } 
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
}