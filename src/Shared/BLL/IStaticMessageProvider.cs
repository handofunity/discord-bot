namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;

    public interface IStaticMessageProvider
    {
        Task EnsureStaticMessagesExist();
    }
}