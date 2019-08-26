namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;

    public interface IVoiceChannelManager
    {
        Task<string> CreateVoiceChannel(string name,
                                        int maxUsers);
    }
}