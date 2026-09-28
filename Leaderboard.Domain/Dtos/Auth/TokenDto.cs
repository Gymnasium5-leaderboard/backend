namespace Leaderboard.Domain.Dtos.Auth;

public record TokenDto(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken,
    DateTime RefreshTokenExpiresAt);
