namespace HoU.GuildBot.Shared.DAL
{
    using System.Threading.Tasks;

    public interface IWebAccess
    {
        Task<byte[]> GetDiscordAvatarByUrl(string url);
    }
}