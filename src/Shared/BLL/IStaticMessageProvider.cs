namespace HoU.GuildBot.Shared.BLL;

public interface IStaticMessageProvider
{
    IDiscordAccess DiscordAccess { set; }

    Task EnsureStaticMessagesExist();
}