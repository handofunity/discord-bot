namespace HoU.GuildBot.Shared.Objects;

public class DiscordLeaderboardResponse
{
    public List<DiscordLeaderboardPositionModel> LeaderboardPositions { get; set; } = [];

    public string? Season { get; set; }
}

