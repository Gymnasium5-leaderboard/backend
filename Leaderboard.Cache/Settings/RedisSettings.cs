namespace Leaderboard.Cache.Settings;

public class RedisSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Password { get; set; }
    public int TimeToLiveInSeconds { get; set; }

    // Short: leaderboards are removed after every change, the TTL only limits a missed removal
    public int LeaderboardTimeToLiveInSeconds { get; set; }
}