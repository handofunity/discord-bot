namespace HoU.GuildBot.BLL
{
    using System;
    using System.Threading.Tasks;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    public class VoiceChannelManager : IVoiceChannelManager
    {
        private readonly IDiscordAccess _discordAccess;
        private readonly AppSettings _appSettings;

        public VoiceChannelManager(IDiscordAccess discordAccess,
                                   AppSettings appSettings)
        {
            _discordAccess = discordAccess;
            _appSettings = appSettings;
        }

        async Task<string> IVoiceChannelManager.CreateVoiceChannel(string name,
                                                                   int maxUsers)
        {
            if (maxUsers < 2)
                return "Max users must be at least 2.";

            var (voiceChannelId, error) = await _discordAccess.CreateVoiceChannel(_appSettings.VoiceChannelCategoryId,
                                                                                                             name,
                                                                                                             maxUsers);
            if (error != null)
                return error;

#pragma warning disable CS4014 // Fire & forget
            Task.Run(async () =>
            {
                var checkDelay = new TimeSpan(0, 5, 0);
                bool deleted;
                do
                {
                    await Task.Delay(checkDelay);
                    deleted = await _discordAccess.DeleteVoiceChannelIfEmpty(voiceChannelId);
                } while (!deleted);
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // Fire & forget

            return null;
        }
    }
}