namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using StrongTypes;

    public interface IVoiceChannelManager
    {
        Task<string> CreateVoiceChannel(string name,
                                        int maxUsers);

        Task<string> TryToMuteUsers(DiscordUserID userId,
                                    string mention);

        Task<string> TryToUnmuteUsers(DiscordUserID userId,
                                      string mention);
    }
}