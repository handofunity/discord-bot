namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly]
    public class StaticMessageProvider : IStaticMessageProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IMessageProvider _messageProvider;
        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly IDiscordAccess _discordAccess;
        private readonly AppSettings _appSettings;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public StaticMessageProvider(IMessageProvider messageProvider,
                                     IGameRoleProvider gameRoleProvider,
                                     IDiscordAccess discordAccess,
                                     AppSettings appSettings)
        {
            _messageProvider = messageProvider;
            _gameRoleProvider = gameRoleProvider;
            _discordAccess = discordAccess;
            _appSettings = appSettings;

            _messageProvider.MessageChanged += MessageProvider_MessageChanged;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private async Task LoadWelcomeChannelMessages(Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)> expectedChannelMessages)
        {
            var l = new List<string>
            {
                await _messageProvider.GetMessage(Constants.MessageNames.WelcomeChannelMessage01).ConfigureAwait(false),
                await _messageProvider.GetMessage(Constants.MessageNames.WelcomeChannelMessage02).ConfigureAwait(false),
                await _messageProvider.GetMessage(Constants.MessageNames.WelcomeChannelMessage03).ConfigureAwait(false),
                await _messageProvider.GetMessage(Constants.MessageNames.WelcomeChannelMessage04).ConfigureAwait(false)
            };
            expectedChannelMessages[_appSettings.WelcomeChannelId] = (l, null);
        }

        private async Task LoadAocRoleMenuMessages(Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)> expectedChannelMessages)
        {
            var l = new List<string>
            {
                await _messageProvider.GetMessage(Constants.MessageNames.AocRoleMenu).ConfigureAwait(false)
            };
            expectedChannelMessages[_appSettings.AshesOfCreationRoleChannelId] = (l, EnsureAocRoleMenuReactionsExist);
        }

        private async Task CreateMessagesInChannel(DiscordChannelID channelID, List<string> messages, Func<ulong[], Task> postCreationCallback)
        {
            await _discordAccess.DeleteBotMessagesInChannel(channelID).ConfigureAwait(false);
            var messageIds = await _discordAccess.CreateBotMessagesInChannel(channelID, messages.ToArray()).ConfigureAwait(false);
            if (postCreationCallback != null)
                await postCreationCallback.Invoke(messageIds).ConfigureAwait(false);
        }

        private async Task EnsureAocRoleMenuReactionsExist(ulong[] messageIds)
        {
            if (messageIds.Length != 1)
                throw new ArgumentException("Unexpected amount of message IDs received.", nameof(messageIds));

            var roleMenuMessageId = messageIds[0];
            await _discordAccess.AddReactionsToMessage(_appSettings.AshesOfCreationRoleChannelId,
                roleMenuMessageId,
                new[]
                {
                    Constants.AocRoleEmojis.Bard,
                    Constants.AocRoleEmojis.Cleric,
                    Constants.AocRoleEmojis.Fighter,
                    Constants.AocRoleEmojis.Mage,
                    Constants.AocRoleEmojis.Ranger,
                    Constants.AocRoleEmojis.Rogue,
                    Constants.AocRoleEmojis.Summoner,
                    Constants.AocRoleEmojis.Tank
                }).ConfigureAwait(false);

            _gameRoleProvider.AocGameRoleMenuMessageID = roleMenuMessageId;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IStaticMessageProvider Members

        async Task IStaticMessageProvider.EnsureStaticMessagesExist()
        {
            var expectedChannelMessages = new Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)>();
            await LoadWelcomeChannelMessages(expectedChannelMessages).ConfigureAwait(false);
            await LoadAocRoleMenuMessages(expectedChannelMessages).ConfigureAwait(false);

            foreach (var pair in expectedChannelMessages)
            {
                var existingMessages = await _discordAccess.GetBotMessagesInChannel(pair.Key).ConfigureAwait(false);
                if (existingMessages.Length != pair.Value.Messages.Count)
                {
                    // If the count is different, we don't have to check every message
                    await CreateMessagesInChannel(pair.Key, pair.Value.Messages, pair.Value.PostCreationCallback).ConfigureAwait(false);
                }
                // If the count is the same, check if all messages are the same, in the correct order
                else if (pair.Value.Messages.Where((t, i) => t != existingMessages[i].Content).Any())
                {
                    // If there is any message that is not at the same position and equal, we re-create all of them
                    await CreateMessagesInChannel(pair.Key, pair.Value.Messages, pair.Value.PostCreationCallback).ConfigureAwait(false);
                }
                else
                {
                    // If the count is the same, and all messages are the same, we have to provide some static data to other classes
                    if (pair.Key == _appSettings.AshesOfCreationRoleChannelId)
                    {
                        _gameRoleProvider.AocGameRoleMenuMessageID = existingMessages[0].MessageID;
                    }
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
                    var expectedChannelMessages = new Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)>();
                    await LoadWelcomeChannelMessages(expectedChannelMessages).ConfigureAwait(false);
                    var (messages, postCreationCallback) = expectedChannelMessages[_appSettings.WelcomeChannelId];
                    await CreateMessagesInChannel(_appSettings.WelcomeChannelId, messages, postCreationCallback).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            else if (e.MessageName == Constants.MessageNames.AocRoleMenu)
            {
                Task.Run(async () =>
                {
                    var expectedChannelMessages = new Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)>();
                    await LoadAocRoleMenuMessages(expectedChannelMessages).ConfigureAwait(false);
                    var (messages, postCreationCallback) = expectedChannelMessages[_appSettings.AshesOfCreationRoleChannelId];
                    await CreateMessagesInChannel(_appSettings.AshesOfCreationRoleChannelId, messages, postCreationCallback).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
        }

        #endregion
    }
}