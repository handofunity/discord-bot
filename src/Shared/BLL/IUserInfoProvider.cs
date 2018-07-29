namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using Objects;
    using StrongTypes;

    public interface IUserInfoProvider
    {
        Task<EmbedData> GetLastSeenInfo();

        EmbedData WhoIs(DiscordUserID userID);

        EmbedData WhoIs(string username, string remainderContent);
    }
}