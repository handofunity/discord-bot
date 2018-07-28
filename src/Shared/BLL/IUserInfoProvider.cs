namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using Objects;

    public interface IUserInfoProvider
    {
        Task<EmbedData> GetLastSeenInfo();
    }
}