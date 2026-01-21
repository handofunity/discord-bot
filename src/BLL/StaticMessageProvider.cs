namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class StaticMessageProvider : IStaticMessageProvider
{
    private readonly Dictionary<DiscordChannelId, SemaphoreSlim> _channelSemaphores;
    private readonly IMessageProvider _messageProvider;
    private readonly IGameRoleProvider _gameRoleProvider;
    private readonly ILogger<StaticMessageProvider> _logger;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IMenuRegistry _menuRegistry;
    private readonly bool _provideStaticMessages;

    private IDiscordAccess? _discordAccess;

    public StaticMessageProvider(IMessageProvider messageProvider,
                                 IGameRoleProvider gameRoleProvider,
                                 IBotInformationProvider botInformationProvider,
                                 ILogger<StaticMessageProvider> logger,
                                 IDynamicConfiguration dynamicConfiguration,
                                 IMenuRegistry menuRegistry)
    {
        _channelSemaphores = new Dictionary<DiscordChannelId, SemaphoreSlim>();
        _messageProvider = messageProvider;
        _gameRoleProvider = gameRoleProvider;
        _logger = logger;
        _dynamicConfiguration = dynamicConfiguration;
        _menuRegistry = menuRegistry;
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

    private async Task LoadInfosAndRolesMenuMessages(IDictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.FriendOrGuestMenuMessage)),
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.GameInterestMenuMessage))
        };
        AddFriendOrGuestMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["InfoAndRolesChannelId"]] =
            ("Infos and roles", new ExpectedChannelMessages(l));
    }

    private async Task LoadGamesRolesMenuMessages(IDictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.GamesRolesMenuMessage))
        };
        AddGamesRolesMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["GamesRolesChannelId"]] =
            ("Game roles", new ExpectedChannelMessages(l));
    }

    private async Task LoadAocRoleMenuMessages(IDictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.AocPlayStyleMenuMessage)),
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.AocRaceMenuMessage)),
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.AocRolePreferenceMenuMessage))
        };
        AddAocRoleMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["AshesOfCreationRoleChannelId"]] =
            ("Ashes of Creation roles", new ExpectedChannelMessages(l));
    }

    private async Task LoadWowRoleMenuMessages(IDictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.WowRoleMenuMessage))
        };
        AddWowRoleMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["WorldOfWarcraftRoleChannelId"]] =
            ("World of Warcraft Classic roles", new ExpectedChannelMessages(l));
    }

    private async Task LoadWowRetailRoleMenuMessages(IDictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.WowRetailPlayStyleMenuMessage))
        };
        AddWowRetailRoleMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["WorldOfWarcraftRetailRoleChannelId"]] =
            ("World of Warcraft Retail roles", new ExpectedChannelMessages(l));
    }

    private async Task LoadLostArkRoleMenuMessages(IDictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)> expectedChannelMessages)
    {
        if (!_provideStaticMessages)
            return;

        var l = new List<ExpectedChannelMessage>
        {
            new(await _messageProvider.GetMessageAsync(Constants.MessageNames.LostArkPlayStyleMenuMessage))
        };
        AddLostArkRoleMenuComponents(l);
        expectedChannelMessages[(DiscordChannelId)_dynamicConfiguration.DiscordMapping["LostArkRoleChannelId"]] =
            (("Lost Ark roles"), new ExpectedChannelMessages(l));
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
            await DiscordAccess.DeleteBotMessagesInChannelAsync(channelID);
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
        messages[0].Components.Add(_menuRegistry.GetButtonMenuComponent(Constants.FriendOrGuestMenu.GuestCustomId));
        messages[0].Components.Add(_menuRegistry.GetButtonMenuComponent(Constants.FriendOrGuestMenu.FriendOfMemberCustomId));

        // Game interest menu
        messages[1].Components.AddRange(_menuRegistry.GetSelectMenuComponents(Constants.GameInterestMenu.CustomId));
    }

    private void AddGamesRolesMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 1)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        messages[0].Components.AddRange(_menuRegistry.GetSelectMenuComponents(Constants.GameRoleMenu.CustomId));
    }

    private void AddAocRoleMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 3)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        // Play style menu
        messages[0].Components.AddRange(_menuRegistry.GetSelectMenuComponents(Constants.AocPlayStyleMenu.CustomId));

        // Race menu
        messages[1].Components.AddRange(_menuRegistry.GetSelectMenuComponents(Constants.AocRaceMenu.CustomId));

        // Role preference menu
        messages[2].Components.AddRange(_menuRegistry.GetSelectMenuComponents(Constants.AocRolePreferenceMenu.CustomId));
    }

    private void AddWowRoleMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 1)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        // Class menu
        messages[0].Components.AddRange(_menuRegistry.GetSelectMenuComponents(Constants.WowClassMenu.CustomId));
    }

    private void AddWowRetailRoleMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 1)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        // Play style menu
        messages[0].Components.AddRange(_menuRegistry.GetSelectMenuComponents(Constants.WowRetailPlayStyleMenu.CustomId));
    }

    private void AddLostArkRoleMenuComponents(List<ExpectedChannelMessage> messages)
    {
        if (messages.Count != 1)
            throw new ArgumentException("Unexpected amount of messages received.", nameof(messages));

        // Play style menu
        messages[0].Components.AddRange(_menuRegistry.GetSelectMenuComponents(Constants.LostArkPlayStyleMenu.CustomId));
    }

    private void ReCreateGameRoleMenuMessages()
    {
        Task.Run(async () =>
        {
            var gamesRolesChannelId = (DiscordChannelId)_dynamicConfiguration.DiscordMapping["GamesRolesChannelId"];
            var expectedChannelMessages = new Dictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)>();
            await LoadGamesRolesMenuMessages(expectedChannelMessages);
            var messages = expectedChannelMessages[gamesRolesChannelId];
            await CreateMessagesInChannel(gamesRolesChannelId, messages.Messages);
        }).ConfigureAwait(false);
    }

    private void ReCreateGameInterestMessages()
    {
        Task.Run(async () =>
        {
            var infosAndRolesChannelId = (DiscordChannelId)_dynamicConfiguration.DiscordMapping["InfoAndRolesChannelId"];
            var expectedChannelMessages = new Dictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)>();
            await LoadInfosAndRolesMenuMessages(expectedChannelMessages);
            var messages = expectedChannelMessages[infosAndRolesChannelId];
            await CreateMessagesInChannel(infosAndRolesChannelId, messages.Messages);
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
        var expectedChannelMessages = new Dictionary<DiscordChannelId, (string Context, ExpectedChannelMessages Messages)>();
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
            if (channelLocationAndName is null)
            {
                await DiscordAccess.LogToDiscordAsync($"{DiscordAccess.GetLeadershipMention()} "
                                                    + $"Failed to find text channel for '{pair.Value.Context}' (ChannelId: {pair.Key}).");
                continue;
            }
            _logger.LogInformation("Loading existing messages for channel '{Channel}' ...", channelLocationAndName);
            var existingMessages = await DiscordAccess.GetBotMessagesInChannelAsync(pair.Key);
            if (existingMessages.Length != pair.Value.Messages.Messages.Length)
            {
                // If the count of messages or action components is different, we don't have to check every message/action component.
                _logger.LogInformation("Messages in channel '{Channel}' are incomplete or too many ({ExistingMessages}/{ExpectedMessages})",
                                       channelLocationAndName,
                                       existingMessages.Length,
                                       pair.Value.Messages.Messages.Length);
                await CreateMessagesInChannel(pair.Key, pair.Value.Messages);
            }
            // If the count is the same, check if all messages are the same, in the correct order
            else if (pair.Value.Messages.Messages.Where((t, i) => t.Content != existingMessages[i].Content).Any())
            {
                // If there is any message that is not at the same position and equal, we re-create all of them
                _logger.LogInformation("Messages in channel '{Channel}' are in the wrong order or have the wrong content", channelLocationAndName);
                await CreateMessagesInChannel(pair.Key, pair.Value.Messages);
            }
            // If the messages are OK, we need to check the action components and options for correctness.
            else if (AreActionComponentsCorrect(existingMessages, pair.Value.Messages.Messages))
            {
                // If the count is the same, and all messages are the same, and the action components are correct, provide existing custom ids to dependent classes.
                if (pair.Key == gamesRolesChannelId)
                {
                    var existingPartialCustomIds = existingMessages.SelectMany(m => m.CustomIdsAndOptions.Keys).ToArray();
                    _menuRegistry.UpdateExistingPartialCustomIds(Constants.GameRoleMenu.CustomId, existingPartialCustomIds);
                }
                _logger.LogInformation("Messages in channel '{Channel}' are correct", channelLocationAndName);
            }
            else
            {
                // If the actions components or options are not correct, we need to re-create all of them.
                _logger.LogInformation("Action components or options in channel '{Channel}' have the wrong count, are in the wrong order or have the wrong content",
                                       channelLocationAndName);
                await CreateMessagesInChannel(pair.Key, pair.Value.Messages);
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

            foreach (var buttonComponent in pair.expectedMessage.Components.OfType<ButtonComponent>())
            {
                // Get existing button properties for the expected custom id.
                if (!pair.existingMessage.CustomIdsAndOptions.TryGetValue(buttonComponent.CustomId, out var buttonProperties)
                 || buttonProperties is null)
                    return false;

                // Button components should have the same label
                if (!buttonProperties.TryGetValue(nameof(Shared.Objects.ButtonComponent.Label), out var existingLabel)
                    || buttonComponent.Label != existingLabel)
                    return false;
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