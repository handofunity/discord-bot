using System;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.StrongTypes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;

namespace HoU.GuildBot.BLL
{
    public class VoiceChannelManager : IVoiceChannelManager
    {
        private const string InsufficientPermissionsMessage = "The bot has insufficient permissions for your current voice channel.";

        private readonly IDiscordAccess _discordAccess;
        private readonly IDynamicConfiguration _dynamicConfiguration;

        public VoiceChannelManager(IDiscordAccess discordAccess,
                                   IDynamicConfiguration dynamicConfiguration)
        {
            _discordAccess = discordAccess;
            _dynamicConfiguration = dynamicConfiguration;
        }

        async Task<string?> IVoiceChannelManager.CreateVoiceChannel(string name,
                                                                    int maxUsers)
        {
            if (maxUsers < 2)
                return "Max users must be at least 2.";

            var (voiceChannelId, error) = await _discordAccess.CreateVoiceChannel(_dynamicConfiguration.DiscordMapping["VoiceChannelCategoryId"],
                                                                                  name,
                                                                                  maxUsers);
            if (error != null)
                return error;

            _ = Task.Run(async () =>
            {
                var checkDelay = new TimeSpan(0, 5, 0);
                bool deleted;
                do
                {
                    await Task.Delay(checkDelay);
                    deleted = await _discordAccess.DeleteVoiceChannelIfEmpty(voiceChannelId);
                } while (!deleted);
            }).ConfigureAwait(false);

            return null;
        }

        async Task<string?> IVoiceChannelManager.TryToMuteUsers(DiscordUserID userId,
                                                                string mention)
        {
            var userVoiceChannelId = _discordAccess.GetUsersVoiceChannelId(userId);
            if (userVoiceChannelId == null)
                return null;

            var didSetMuteState = await _discordAccess.SetUsersMuteStateInVoiceChannel(userVoiceChannelId.Value, true)
                                                      .ConfigureAwait(false);

            return didSetMuteState
                       ? null
                       : $"{mention}: " + InsufficientPermissionsMessage;
        }

        async Task<string?> IVoiceChannelManager.TryToUnMuteUsers(DiscordUserID userId,
                                                                  string mention)
        {
            var userVoiceChannelId = _discordAccess.GetUsersVoiceChannelId(userId);
            if (userVoiceChannelId == null)
                return null;

            var didSetMuteState = await _discordAccess.SetUsersMuteStateInVoiceChannel(userVoiceChannelId.Value, false)
                                                      .ConfigureAwait(false);

            return didSetMuteState
                       ? null
                       : $"{mention}: " + InsufficientPermissionsMessage;
        }
    }
}