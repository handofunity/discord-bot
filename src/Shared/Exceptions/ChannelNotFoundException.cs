namespace HoU.GuildBot.Shared.Exceptions;

public class ChannelNotFoundException : Exception
{
    public ChannelNotFoundException(DiscordChannelId channelId) : base($"Couldn't find channel with the ID '{channelId}'.")
    {
    }
}