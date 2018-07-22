namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using DAL;
    using Objects;
    using StrongTypes;

    public interface IGuildUserInspector
    {
        /// <summary>
        /// Sets the <see cref="IDiscordAccess"/> instance.
        /// </summary>
        IDiscordAccess DiscordAccess { set; }

        Task<EmbedData> GetLastSeenInfo();

        Task UpdateLastSeenInfo(DiscordUserID userID, bool wasOnline, bool isOnline);
    }
}