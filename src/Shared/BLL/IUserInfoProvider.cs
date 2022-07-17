namespace HoU.GuildBot.Shared.BLL;

public interface IUserInfoProvider
{
    Task<string[]> GetLastSeenInfo();

    EmbedData WhoIs(DiscordUserId userID);

    EmbedData WhoIs(InternalUserId userID);
}