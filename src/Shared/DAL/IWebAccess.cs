using System.Threading.Tasks;

namespace HoU.GuildBot.Shared.DAL
{
    public interface IWebAccess
    {
        Task<byte[]> GetDiscordAvatarByUrl(string url);
    }
}