namespace HoU.GuildBot.Shared.Objects;

public class VoiceChannelAttendees
{
    public string DiscordVoiceChannelId { get; }

    public List<DiscordUserId> DiscordUserIds { get; }

    public VoiceChannelAttendees(string discordVoiceChannelId,
                                 List<DiscordUserId> discordUserIds)
    {
        DiscordVoiceChannelId = discordVoiceChannelId;
        DiscordUserIds = discordUserIds;
    }
}