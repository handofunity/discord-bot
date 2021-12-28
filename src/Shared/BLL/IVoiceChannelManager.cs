using System.Threading.Tasks;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IVoiceChannelManager
    {
        Task<string?> CreateVoiceChannel(string name,
                                         int maxUsers);

        Task<string?> TryToMuteUsers(DiscordUserID userId,
                                     string mention);

        Task<string?> TryToUnMuteUsers(DiscordUserID userId,
                                       string mention);
    }
}