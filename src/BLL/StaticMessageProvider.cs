namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly]
    public class StaticMessageProvider : IStaticMessageProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IMessageProvider _messageProvider;
        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly AppSettings _appSettings;
        private readonly bool _provideStaticMessages;

        private IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public StaticMessageProvider(IMessageProvider messageProvider,
                                     IGameRoleProvider gameRoleProvider,
                                     IBotInformationProvider botInformationProvider,
                                     AppSettings appSettings)
        {
            _messageProvider = messageProvider;
            _gameRoleProvider = gameRoleProvider;
            _appSettings = appSettings;
#if DEBUG
            _provideStaticMessages = botInformationProvider.GetEnvironmentName() == Constants.RuntimeEnvironment.Production;
#else
            // Don't change this statement, or the bot might not behave the way it should in the production environment.
            _provideStaticMessages = botInformationProvider.GetEnvironmentName() == Constants.RuntimeEnvironment.Production;
#endif

            if (_provideStaticMessages)
            {
                _messageProvider.MessageChanged += MessageProvider_MessageChanged;
                _gameRoleProvider.GameChanged += GameRoleProvider_GameChanged;
            }
        }

#endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
#region Private Methods

        private async Task LoadAocRoleMenuMessages(Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)> expectedChannelMessages)
        {
            if (!_provideStaticMessages)
                return;

            var l = new List<string>
            {
                await _messageProvider.GetMessage(Constants.MessageNames.AocRoleMenu).ConfigureAwait(false)
            };
            expectedChannelMessages[_appSettings.AshesOfCreationRoleChannelId] = (l, EnsureAocRoleMenuReactionsExist);
        }

        private async Task LoadWowRoleMenuMessages(Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)> expectedChannelMessages)
        {
            if (!_provideStaticMessages)
                return;

            var l = new List<string>
            {
                await _messageProvider.GetMessage(Constants.MessageNames.WowRoleMenu).ConfigureAwait(false)
            };
            expectedChannelMessages[_appSettings.WorldOfWarcraftRoleChannelId] = (l, EnsureWowRoleMenuReactionsExist);
        }

        private async Task LoadGamesRolesMenuMessages(Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)> expectedChannelMessages)
        {
            if (!_provideStaticMessages)
                return;

            var messageTemplate = await _messageProvider.GetMessage(Constants.MessageNames.GamesRolesMenu).ConfigureAwait(false);
            var l = _gameRoleProvider.Games
                                      // Only those games with a PrimaryGameDiscordRoleID and the flag IncludeInGamesMenu enabled can be used here
                                     .Where(m => m.PrimaryGameDiscordRoleID != null && m.IncludeInGamesMenu)
                                     .OrderBy(m => m.LongName)
                                     .Select(m => string.Format(messageTemplate, m.LongName))
                                     .ToList();
            expectedChannelMessages[_appSettings.GamesRolesChannelId] = (l, EnsureGamesRolesMenuReactionsExist);
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

        private async Task EnsureWowRoleMenuReactionsExist(ulong[] messageIds)
        {
            if (messageIds.Length != 1)
                throw new ArgumentException("Unexpected amount of message IDs received.", nameof(messageIds));

            var roleMenuMessageId = messageIds[0];
            await _discordAccess.AddReactionsToMessage(_appSettings.WorldOfWarcraftRoleChannelId,
                                                       roleMenuMessageId,
                                                       new[]
                                                       {
                                                           Constants.WowRoleEmojis.Druid,
                                                           Constants.WowRoleEmojis.Hunter,
                                                           Constants.WowRoleEmojis.Mage,
                                                           Constants.WowRoleEmojis.Paladin,
                                                           Constants.WowRoleEmojis.Priest,
                                                           Constants.WowRoleEmojis.Rogue,
                                                           Constants.WowRoleEmojis.Warlock,
                                                           Constants.WowRoleEmojis.Warrior
                                                       }).ConfigureAwait(false);

            _gameRoleProvider.WowGameRoleMenuMessageID = roleMenuMessageId;
        }

        private async Task EnsureGamesRolesMenuReactionsExist(ulong[] messageIds)
        {
            if (messageIds.Length != _gameRoleProvider.Games.Count(m => m.PrimaryGameDiscordRoleID != null))
                throw new ArgumentException("Unexpected amount of message IDs received.", nameof(messageIds));

            foreach (var messageId in messageIds)
            {
                await _discordAccess.AddReactionsToMessage(_appSettings.GamesRolesChannelId, messageId, new[] {Constants.GamesRolesEmojis.Joystick}).ConfigureAwait(false);
                await Task.Delay(500).ConfigureAwait(false);
            }

            _gameRoleProvider.GamesRolesMenuMessageIDs = messageIds.ToArray();
        }

        private void ReCreateGameRoleMenuMessages()
        {
            Task.Run(async () =>
            {
                var expectedChannelMessages = new Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)>();
                await LoadGamesRolesMenuMessages(expectedChannelMessages).ConfigureAwait(false);
                var (messages, postCreationCallback) = expectedChannelMessages[_appSettings.GamesRolesChannelId];
                await CreateMessagesInChannel(_appSettings.GamesRolesChannelId, messages, postCreationCallback).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

#endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
#region IStaticMessageProvider Members

        IDiscordAccess IStaticMessageProvider.DiscordAccess
        {
            set => _discordAccess = value;
        }

        async Task IStaticMessageProvider.EnsureStaticMessagesExist()
        {
            var expectedChannelMessages = new Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)>();
            await LoadAocRoleMenuMessages(expectedChannelMessages).ConfigureAwait(false);
            await LoadWowRoleMenuMessages(expectedChannelMessages).ConfigureAwait(false);
            await LoadGamesRolesMenuMessages(expectedChannelMessages).ConfigureAwait(false);

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
                    else if (pair.Key == _appSettings.WorldOfWarcraftRoleChannelId)
                    {
                        _gameRoleProvider.WowGameRoleMenuMessageID = existingMessages[0].MessageID;
                    }
                    else if (pair.Key == _appSettings.GamesRolesChannelId)
                    {
                        _gameRoleProvider.GamesRolesMenuMessageIDs = existingMessages.Select(m => m.MessageID).ToArray();
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

            if (e.MessageName == Constants.MessageNames.AocRoleMenu)
            {
                Task.Run(async () =>
                {
                    var expectedChannelMessages = new Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)>();
                    await LoadAocRoleMenuMessages(expectedChannelMessages).ConfigureAwait(false);
                    var (messages, postCreationCallback) = expectedChannelMessages[_appSettings.AshesOfCreationRoleChannelId];
                    await CreateMessagesInChannel(_appSettings.AshesOfCreationRoleChannelId, messages, postCreationCallback).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            else if (e.MessageName == Constants.MessageNames.WowRoleMenu)
            {
                Task.Run(async () =>
                {
                    var expectedChannelMessages = new Dictionary<DiscordChannelID, (List<string> Messages, Func<ulong[], Task> PostCreationCallback)>();
                    await LoadWowRoleMenuMessages(expectedChannelMessages).ConfigureAwait(false);
                    var (messages, postCreationCallback) = expectedChannelMessages[_appSettings.WorldOfWarcraftRoleChannelId];
                    await CreateMessagesInChannel(_appSettings.WorldOfWarcraftRoleChannelId, messages, postCreationCallback).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            else if (e.MessageName == Constants.MessageNames.GamesRolesMenu)
            {
                ReCreateGameRoleMenuMessages();
            }
        }

        private void GameRoleProvider_GameChanged(object sender, GameChangedEventArgs e)
        {
            // Add has never a primary game role ID
            if (e.GameModification == GameModification.Edited
             || e.GameModification == GameModification.Removed)
            {
                ReCreateGameRoleMenuMessages();
            }
        }

#endregion
    }
}