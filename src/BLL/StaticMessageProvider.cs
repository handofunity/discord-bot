namespace HoU.GuildBot.BLL
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    [UsedImplicitly]
    public class StaticMessageProvider : IStaticMessageProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IMessageProvider _messageProvider;
        private readonly IDiscordAccess _discordAccess;
        private readonly AppSettings _appSettings;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public StaticMessageProvider(IMessageProvider messageProvider,
                                     IDiscordAccess discordAccess,
                                     AppSettings appSettings)
        {
            _messageProvider = messageProvider;
            _discordAccess = discordAccess;
            _appSettings = appSettings;

            _messageProvider.MessageChanged += MessageProvider_MessageChanged;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private async Task LoadWelcomeChannelMessages(Dictionary<ulong, List<string>> expectedChannelMessages)
        {
            var l = new List<string>
            {
                await _messageProvider.GetMessage(Constants.MessageNames.WelcomeChannelMessage01).ConfigureAwait(false),
                await _messageProvider.GetMessage(Constants.MessageNames.WelcomeChannelMessage02).ConfigureAwait(false),
                await _messageProvider.GetMessage(Constants.MessageNames.WelcomeChannelMessage03).ConfigureAwait(false),
                await _messageProvider.GetMessage(Constants.MessageNames.WelcomeChannelMessage04).ConfigureAwait(false)
            };
            expectedChannelMessages[_appSettings.WelcomeChannelId] = l;
        }

        private async Task CreateMessagesInChannel(ulong channelID, List<string> messages)
        {
            await _discordAccess.DeleteBotMessagesInChannel(channelID).ConfigureAwait(false);
            await _discordAccess.CreateBotMessagesInChannel(channelID, messages.ToArray()).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IStaticMessageProvider Members

        async Task IStaticMessageProvider.EnsureStaticMessagesExist()
        {
            var expectedChannelMessages = new Dictionary<ulong, List<string>>();
            await LoadWelcomeChannelMessages(expectedChannelMessages).ConfigureAwait(false);

            foreach (var pair in expectedChannelMessages)
            {
                var existingMessages = await _discordAccess.GetBotMessagesInChannel(pair.Key).ConfigureAwait(false);
                if (existingMessages.Length != pair.Value.Count)
                {
                    // If the count is different, we don't have to check every message
                    await CreateMessagesInChannel(pair.Key, pair.Value).ConfigureAwait(false);
                    continue;
                }

                // If the count is the same, check if all messages are the same, in the correct order
                if (pair.Value.Where((t, i) => t != existingMessages[i]).Any())
                {
                    // If there is any message that is not at the same position and equal, we re-create all of them
                    await CreateMessagesInChannel(pair.Key, pair.Value).ConfigureAwait(false);
                }
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Event Handler

        private void MessageProvider_MessageChanged(object sender, MessageChangedEventArgs e)
        {
            // We can just create the messages here, because the message was changed.
            // There's no need to check if the messages in the channel are the same.

            if (e.MessageName == Constants.MessageNames.WelcomeChannelMessage01
             || e.MessageName == Constants.MessageNames.WelcomeChannelMessage02
             || e.MessageName == Constants.MessageNames.WelcomeChannelMessage03
             || e.MessageName == Constants.MessageNames.WelcomeChannelMessage04)
            {
                Task.Run(async () =>
                {
                    var expectedChannelMessages = new Dictionary<ulong, List<string>>();
                    await LoadWelcomeChannelMessages(expectedChannelMessages).ConfigureAwait(false);
                    await CreateMessagesInChannel(_appSettings.WelcomeChannelId, expectedChannelMessages[_appSettings.WelcomeChannelId]).ConfigureAwait(false);
                });
            }
        }

        #endregion
    }
}