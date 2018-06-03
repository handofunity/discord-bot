namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using DAL;
    using Objects;

    public interface IGuildUserInspector
    {
        /// <summary>
        /// Sets the <see cref="IDiscordAccess"/> instance.
        /// </summary>
        IDiscordAccess DiscordAccess { set; }

        Task<EmbedData> GetLastSeenInfo();

        Task UpdateLastSeenInfo(ulong userID, bool wasOnline, bool isOnline);
    }
}