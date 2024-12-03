namespace HoU.GuildBot.Shared.Objects;

public class SyncCreatedVoiceChannelsRequest
{
    public int AppointmentId { get; }

    public ulong DiscordCategoryChannelId { get; }

    public List<EventVoiceChannel> CreatedVoiceChannels { get; }

    public SyncCreatedVoiceChannelsRequest(int appointmentId,
        DiscordCategoryChannelId discordCategoryChannelId,
        List<EventVoiceChannel> createdVoiceChannels)
    {
        AppointmentId = appointmentId;
        DiscordCategoryChannelId = (ulong)discordCategoryChannelId;
        CreatedVoiceChannels = createdVoiceChannels;
    }
}