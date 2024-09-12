namespace HoU.GuildBot.Shared.Objects;

public class VoiceChannelAttendees
{
    public string DiscordVoiceChannelId { get; }

    public List<ulong> DiscordUserIds { get; }

    public VoiceChannelAttendees(string discordVoiceChannelId,
                                 List<ulong> discordUserIds)
    {
        DiscordVoiceChannelId = discordVoiceChannelId;
        DiscordUserIds = discordUserIds;
    }
}