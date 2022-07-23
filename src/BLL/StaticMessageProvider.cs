namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class StaticMessageProvider : IStaticMessageProvider
{
    private readonly Dictionary<DiscordChannelId, SemaphoreSlim> _channelSemaphores;
    private readonly IMessageProvider _messageProvider;
    private readonly IGameRoleProvider _gameRoleProvider;
    private readonly ILogger<StaticMessageProvider> _logger;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly bool _provideStaticMessages;

    private IDiscordAccess? _discordAccess;
        
    public StaticMessageProvider(IMessageProvider messageProvider,
                                 IGameRoleProvider gameRoleProvider,
                                 IBotInformationProvider botInformationProvider,
                                 ILogger<StaticMessageProvider> logger,
                                 IDynamicConfiguration dynamicConfiguration)
    {
        _channelSemaphores = new Dictionary<DiscordChannelId, SemaphoreSlim>();
        _messageProvider = messageProvider;
        _gameRoleProvider = gameRoleProvider;
        _logger = logger;
        _dynamicConfiguration = dynamicConfiguration;
#if DEBUG
        _provideStaticMessages = botInformationProvider.GetEnvironmentName() == Constants.RuntimeEnvironment.Production;
#else
        // Don't change this statement, or the bot might not behave the way it should in the production environment.
        _provideStaticMessages = botInformationProvider.GetEnvironmentName() == Constants.RuntimeEnvironment.Production;
#endif

        if (_provideStaticMessages)
        {
            _gameRoleProvider.GameChanged += GameRoleProvider_GameChanged;
        }
    }

    private async Task LoadInfosAndRolesMenuMessages(IDictionary<DiscordChannelId, ExpectedChannelMessages> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessage(Constants.MessageNames.FriendOrGuestMenuMessage)),
            new(await _messageProvider.GetMessage(Constants.MessageNames.GameInterestMenuMessage))
        };
        AddFriendOrGuestMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["InfoAndRolesChannelId"]] = new ExpectedChannelMessages(l);
    }

    private async Task LoadGamesRolesMenuMessages(IDictionary<DiscordChannelId, ExpectedChannelMessages> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;
        
        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessage(Constants.MessageNames.GamesRolesMenuMessage))
        };
        AddGamesRolesMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["GamesRolesChannelId"]] = new ExpectedChannelMessages(l);
    }

    private async Task LoadAocRoleMenuMessages(IDictionary<DiscordChannelId, ExpectedChannelMessages> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessage(Constants.MessageNames.AocClassMenuMessage)),
            new(await _messageProvider.GetMessage(Constants.MessageNames.AocPlayStyleMenuMessage)),
            new(await _messageProvider.GetMessage(Constants.MessageNames.AocRaceMenuMessage)),
            new(await _messageProvider.GetMessage(Constants.MessageNames.AocGuildPreferenceMenuMessage))
        };
        AddAocRoleMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["AshesOfCreationRoleChannelId"]] = new ExpectedChannelMessages(l);
    }

    private async Task LoadWowRoleMenuMessages(IDictionary<DiscordChannelId, ExpectedChannelMessages> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessage(Constants.MessageNames.WowRoleMenuMessage))
        };
        AddWowRoleMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["WorldOfWarcraftRoleChannelId"]] = new ExpectedChannelMessages(l);
    }

    private async Task LoadWowRetailRoleMenuMessages(IDictionary<DiscordChannelId, ExpectedChannelMessages> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessage(Constants.MessageNames.WowRetailPlayStyleMenuMessage))
        };
        AddWowRetailRoleMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["WorldOfWarcraftRetailRoleChannelId"]] = new ExpectedChannelMessages(l);
    }

    private async Task LoadLostArkRoleMenuMessages(IDictionary<DiscordChannelId, ExpectedChannelMessages> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessage(Constants.MessageNames.LostArkPlayStyleMenuMessage))
        };
        AddLostArkRoleMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["LostArkRoleChannelId"]] = new ExpectedChannelMessages(l);
    }

    private async Task CreateMessagesInChannel(DiscordChannelId channelID,
                                               ExpectedChannelMessages messages)
    {
        if (!_channelSemaphores.TryGetValue(channelID, out var semaphore))
        {
            semaphore = new SemaphoreSlim(1, 1);
            _channelSemaphores.Add(channelID, semaphore);
        }

        var channelLocationAndName = DiscordAccess.GetChannelLocationAndName(channelID);

        try
        {
            _logger.LogInformation("{Channel} - Waiting for channel-edit-semaphore ...", channelLocationAndName);
            await semaphore.WaitAsync();
            _logger.LogInformation("{Channel} - Got channel-edit-semaphore", channelLocationAndName);
            _logger.LogInformation("{Channel} - Deleting existing bot messages in the channel ...", channelLocationAndName);
            await DiscordAccess.DeleteBotMessagesInChannel(channelID);
            _logger.LogInformation("{Channel} - Creating new messages in the channel ...", channelLocationAndName);
            await DiscordAccess.CreateBotMessagesInChannelAsync(channelID, messages.ToArray());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Channel} - Failed to create all messages for channel", channelLocationAndName);
        }
        finally
        {
            _logger.LogInformation("{Channel} - Releasing channel-edit-semaphore ...", channelLocationAndName);
            semaphore.Release();
            _logger.LogInformation("{Channel} - Channel-edit-semaphore released", channelLocationAndName);
        }
    }

    private void AddFriendOrGuestMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 2)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));
        
        // Friend or Guest menu
        foreach (var (customId, label) in Constants.FriendOrGuestMenu.GetOptions())
        {
            messages[0].Components.Add(new ButtonComponent(customId,
                                                           0,
                                                           label,
                                                           InteractionButtonStyle.Primary));
        }

        // Game interest menu
        DiscordAccess.EnsureDisplayNamesAreSet(_gameRoleProvider.Games);
        var games = _gameRoleProvider.Games
                                     // Only those games with the GameInterestRoleId set can be used here
                                     .Where(m => m.GameInterestRoleId != null)
                                     .OrderBy(m => m.DisplayName)
                                     .Take(25)
                                     .ToDictionary(m => m.GameInterestRoleId.ToString()!, m => m.DisplayName ?? "<UNKNOWN>");
        messages[1].Components.Add(new SelectMenuComponent(Constants.GameInterestMenu.CustomId,
                                                           0,
                                                           "Select game interests ...",
                                                           games,
                                                           true));
    }

    private void AddGamesRolesMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 1)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        DiscordAccess.EnsureDisplayNamesAreSet(_gameRoleProvider.Games);
        var games = _gameRoleProvider.Games
                                      // Only those games with the flag IncludeInGamesMenu enabled can be used here
                                     .Where(m => m.IncludeInGamesMenu)
                                     .OrderBy(m => m.DisplayName)
                                     .Take(125)
                                     .ToDictionary(m => m.PrimaryGameDiscordRoleId.ToString(), m => m.DisplayName ?? "<UNKNOWN>");

        var customIds = Enumerable.Range(0, (int)Math.Ceiling(games.Count / 25m))
                                  .Select(_ => Guid.NewGuid().ToString("D"))
                                  .ToArray();
        _gameRoleProvider.GamesRolesCustomIds = customIds;
        byte actionRowNumber = 0;
        foreach (var customId in customIds)
        {
            messages[0].Components.Add(new SelectMenuComponent(customId,
                                                               actionRowNumber,
                                                               "Select games ...",
                                                               games.Skip(actionRowNumber * 25)
                                                                    .Take(25)
                                                                    .ToDictionary(m => m.Key, m => m.Value),
                                                               true));
            actionRowNumber++;
        }
    }

    private static void AddAocRoleMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 4)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        // Class menu
        messages[0].Components.Add(new SelectMenuComponent(Constants.AocArchetypeMenu.CustomId,
                                                           0,
                                                           "Select archetypes ...",
                                                           Constants.AocArchetypeMenu.GetOptions(),
                                                           true));
        // Play style menu
        messages[1].Components.Add(new SelectMenuComponent(Constants.AocPlayStyleMenu.CustomId,
                                                           0,
                                                           "Select play styles ...",
                                                           Constants.AocPlayStyleMenu.GetOptions(),
                                                           true));
        // Race menu
        messages[2].Components.Add(new SelectMenuComponent(Constants.AocRaceMenu.CustomId,
                                                           0,
                                                           "Select races ...",
                                                           Constants.AocRaceMenu.GetOptions(),
                                                           true));

        // Guild preference menu
        messages[3].Components.Add(new SelectMenuComponent(Constants.AocGuildPreferenceMenu.CustomId,
                                                           0,
                                                           "Select preferred in-game guild ...",
                                                           Constants.AocGuildPreferenceMenu.GetOptions(),
                                                           false));
    }

    private static void AddWowRoleMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 1)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        // Class menu
        messages[0].Components.Add(new SelectMenuComponent(Constants.WowClassMenu.CustomId,
                                                           0,
                                                           "Select classes ...",
                                                           Constants.WowClassMenu.GetOptions(),
                                                           true));
    }

    private static void AddWowRetailRoleMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 1)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        // Class menu
        messages[0].Components.Add(new SelectMenuComponent(Constants.WowRetailPlayStyleMenu.CustomId,
                                                           0,
                                                           "Select play styles ...",
                                                           Constants.WowRetailPlayStyleMenu.GetOptions(),
                                                           true));
    }

    private static void AddLostArkRoleMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 1)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        // Play style menu
        messages[0].Components.Add(new SelectMenuComponent(Constants.LostArkPlayStyleMenu.CustomId,
                                                           0,
                                                           "Select play styles ...",
                                                           Constants.LostArkPlayStyleMenu.GetOptions(),
                                                           true));
    }

    private void ReCreateGameRoleMenuMessages()
    {
        Task.Run(async () =>
        {
            var gamesRolesChannelId = (DiscordChannelId)_dynamicConfiguration.DiscordMapping["GamesRolesChannelId"];
            var expectedChannelMessages = new Dictionary<DiscordChannelId, ExpectedChannelMessages>();
            await LoadGamesRolesMenuMessages(expectedChannelMessages);
            var messages = expectedChannelMessages[gamesRolesChannelId];
            await CreateMessagesInChannel(gamesRolesChannelId, messages);
        }).ConfigureAwait(false);
    }

    private void ReCreateGameInterestMessages()
    {
        Task.Run(async () =>
        {
            var infosAndRolesChannelId = (DiscordChannelId)_dynamicConfiguration.DiscordMapping["InfoAndRolesChannelId"];
            var expectedChannelMessages = new Dictionary<DiscordChannelId, ExpectedChannelMessages>();
            await LoadInfosAndRolesMenuMessages(expectedChannelMessages);
            var messages = expectedChannelMessages[infosAndRolesChannelId];
            await CreateMessagesInChannel(infosAndRolesChannelId, messages);
        }).ConfigureAwait(false);
    }

    public IDiscordAccess DiscordAccess
    {
        set => _discordAccess = value;
        private get => _discordAccess ?? throw new InvalidOperationException();
    }

    async Task IStaticMessageProvider.EnsureStaticMessagesExist()
    {
        _logger.LogInformation("Ensuring that all static messages exist");
        var expectedChannelMessages = new Dictionary<DiscordChannelId, ExpectedChannelMessages>();
        await LoadInfosAndRolesMenuMessages(expectedChannelMessages);
        await LoadGamesRolesMenuMessages(expectedChannelMessages);
        await LoadAocRoleMenuMessages(expectedChannelMessages);
        await LoadWowRoleMenuMessages(expectedChannelMessages);
        await LoadWowRetailRoleMenuMessages(expectedChannelMessages);
        await LoadLostArkRoleMenuMessages(expectedChannelMessages);
        
        var gamesRolesChannelId = (DiscordChannelId)_dynamicConfiguration.DiscordMapping["GamesRolesChannelId"];
        foreach (var pair in expectedChannelMessages)
        {
            var channelLocationAndName = DiscordAccess.GetChannelLocationAndName(pair.Key);
            _logger.LogInformation("Loading existing messages for channel '{Channel}' ...", channelLocationAndName);
            var existingMessages = await DiscordAccess.GetBotMessagesInChannel(pair.Key);
            if (existingMessages.Length != pair.Value.Messages.Length)
            {
                // If the count of messages or action components is different, we don't have to check every message/action component.
                _logger.LogInformation("Messages in channel '{Channel}' are incomplete or too many ({ExistingMessages}/{ExpectedMessages})",
                                       channelLocationAndName,
                                       existingMessages.Length,
                                       pair.Value.Messages.Length);
                await CreateMessagesInChannel(pair.Key, pair.Value);
            }
            // If the count is the same, check if all messages are the same, in the correct order
            else if (pair.Value.Messages.Where((t, i) => t.Content != existingMessages[i].Content).Any())
            {
                // If there is any message that is not at the same position and equal, we re-create all of them
                _logger.LogInformation("Messages in channel '{Channel}' are in the wrong order or have the wrong content", channelLocationAndName);
                await CreateMessagesInChannel(pair.Key, pair.Value);
            }
            // If the messages are OK, we need to check the action components and options for correctness.
            else if (AreActionComponentsCorrect(existingMessages, pair.Value.Messages))
            {
                // If the count is the same, and all messages are the same, and the action components are correct, provide existing custom ids to dependent classes.
                if (pair.Key == gamesRolesChannelId)
                {
                    _gameRoleProvider.GamesRolesCustomIds = existingMessages.SelectMany(m => m.CustomIdsAndOptions.Keys).ToArray();
                }
                _logger.LogInformation("Messages in channel '{Channel}' are correct", channelLocationAndName);
            }
            else
            {
                // If the actions components or options are not correct, we need to re-create all of them.
                _logger.LogInformation("Action components or options in channel '{Channel}' have the wrong count, are in the wrong order or have the wrong content",
                                       channelLocationAndName);
                await CreateMessagesInChannel(pair.Key, pair.Value);
            }
        }
    }

    private static bool AreActionComponentsCorrect(TextMessage[] existingMessages,
                                                   ExpectedChannelMessage[] expectedChannelMessages)
    {
        var match = existingMessages.Join(expectedChannelMessages,
                                          existingMessage => existingMessage.Content,
                                          expectedMessage => expectedMessage.Content,
                                          (existingMessage,
                                           expectedMessage) => new { existingMessage, expectedMessage })
                                    .ToArray();
        foreach (var pair in match)
        {
            if (pair.existingMessage.CustomIdsAndOptions.Count != pair.expectedMessage.Components.Count)
                // If the count of custom ids and expected action components differ, the action components are not correct.
                return false;

            foreach (var selectMenuComponent in pair.expectedMessage.Components.OfType<SelectMenuComponent>())
            {
                // Get existing options for the expected custom id.
                if (!pair.existingMessage.CustomIdsAndOptions.TryGetValue(selectMenuComponent.CustomId, out var existingOptions)
                    || existingOptions == null)
                    return false;

                // Select menu components should have the same length and order of options.
                if (existingOptions.Count != selectMenuComponent.Options.Count)
                    return false;
                for (var i = 0; i < selectMenuComponent.Options.Count; i++)
                {
                    var (actualKey, actualValue) = existingOptions.ElementAt(i);
                    var (expectedKey, expectedValue) = selectMenuComponent.Options.ElementAt(i);
                    if (actualKey != expectedKey || actualValue != expectedValue)
                        return false;
                }
            }
        }

        return true;
    }

    private void GameRoleProvider_GameChanged(object? sender, GameChangedEventArgs e)
    {
        // Add has never a primary game role ID
        if (e.GameModification == GameModification.Edited
         || e.GameModification == GameModification.Removed)
        {
            ReCreateGameRoleMenuMessages();
            ReCreateGameInterestMessages();
        }
    }

    private class ExpectedChannelMessages
    {
        public ExpectedChannelMessage[] Messages { get; }

        public ExpectedChannelMessages(IEnumerable<ExpectedChannelMessage> messages)
        {
            Messages = messages.ToArray();
        }

        public (string Content, ActionComponent[] Components)[] ToArray() =>
            Messages.Select(m => (m.Content, m.Components.ToArray())).ToArray();
    }

    private class ExpectedChannelMessage
    {
        public string Content { get; }

        public List<ActionComponent> Components { get; }

        public ExpectedChannelMessage(string content)
        {
            Content = content;
            Components = new List<ActionComponent>();
        }
    }
}