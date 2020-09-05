using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IStaticMessageProvider
    {
        IDiscordAccess DiscordAccess { set; }

        Task EnsureStaticMessagesExist();
    }
}