namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using DAL;

    public interface IStaticMessageProvider
    {
        IDiscordAccess DiscordAccess { set; }

        Task EnsureStaticMessagesExist();
    }
}