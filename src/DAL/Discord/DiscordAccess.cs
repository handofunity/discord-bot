using ButtonComponent = HoU.GuildBot.Shared.Objects.ButtonComponent;
using SelectMenuComponent = HoU.GuildBot.Shared.Objects.SelectMenuComponent;
using User = HoU.GuildBot.Shared.Objects.User;

namespace HoU.GuildBot.DAL.Discord;

[UsedImplicitly]
public class DiscordAccess : IDiscordAccess
{
    private const string GroupRolePrefix = "●";
    private static readonly Dictionary<string, Role> _roleMapping;
    private readonly ILogger<DiscordAccess> _logger;
    private readonly RootSettings _rootSettings;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISpamGuard _spamGuard;
    private readonly IIgnoreGuard _ignoreGuard;
    private readonly IGameRoleProvider _gameRoleProvider;
    private readonly IStaticMessageProvider _staticMessageProvider;
    private readonly IDiscordUserEventHandler _discordUserEventHandler;
    private readonly IUserStore _userStore;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly Queue<string> _pendingMessages;

    private bool _guildAvailable;
    
    static DiscordAccess()
    {
        _roleMapping = new Dictionary<string, Role>
        {
            {"Developer", Role.Developer},
            {"Leader", Role.Leader},
            {"Officer", Role.Officer},
            {"Community Coordinator", Role.Coordinator},
            {"WoW Classic Coordinator", Role.Coordinator},
            {"Member", Role.Member},
            {"Trial Member", Role.TrialMember},
            {"Guest", Role.Guest},
            {"Friend of Member", Role.FriendOfMember}
        };
    }

    public DiscordAccess(ILogger<DiscordAccess> logger,
                         RootSettings rootSettings,
                         IDynamicConfiguration dynamicConfiguration,
                         IServiceProvider serviceProvider,
                         ISpamGuard spamGuard,
                         IIgnoreGuard ignoreGuard,
                         INonMemberRoleProvider nonMemberRoleProvider,
                         IGameRoleProvider gameRoleProvider,
                         IStaticMessageProvider staticMessageProvider,
                         IDiscordUserEventHandler discordUserEventHandler,
                         IUserStore userStore)
    {
        _logger = logger;
        _rootSettings = rootSettings;
        _dynamicConfiguration = dynamicConfiguration;
        _serviceProvider = serviceProvider;
        _spamGuard = spamGuard;
        _ignoreGuard = ignoreGuard;
        _gameRoleProvider = gameRoleProvider;
        _staticMessageProvider = staticMessageProvider;
        _discordUserEventHandler = discordUserEventHandler;
        _userStore = userStore;
        nonMemberRoleProvider.DiscordAccess = this;
        _gameRoleProvider.DiscordAccess = this;
        _staticMessageProvider.DiscordAccess = this;
        _discordUserEventHandler.DiscordAccess = this;
        var clientConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildMembers
                           | GatewayIntents.GuildBans
                           | GatewayIntents.GuildVoiceStates
                           | GatewayIntents.GuildPresences
                           | GatewayIntents.GuildMessages
        };
        _client = new DiscordSocketClient(clientConfig);
        _interactionService = new InteractionService(_client,
                                                     new InteractionServiceConfig
                                                     {
                                                         UseCompiledLambda = true
                                                     });
        _interactionService.SlashCommandExecuted -= InteractionService_SlashCommandExecuted;
        _interactionService.SlashCommandExecuted += InteractionService_SlashCommandExecuted;
        _pendingMessages = new Queue<string>();
            
        _client.Log += LogClient;
        _interactionService.Log += LogInteractions;
    }

    /// <summary>
    /// Gets the "Hand of Unity" <see cref="SocketGuild"/> object.
    /// </summary>
    /// <returns>A <see cref="SocketGuild"/> instance for "Hand of Unity".</returns>
    private SocketGuild GetGuild() =>
        _client.GetGuild(_dynamicConfiguration.DiscordMapping["ValidGuildId"])
     ?? throw new InvalidOperationException("Guild not found.");

    private bool IsUserOnServer(DiscordUserId userId)
    {
        // Cannot check if the user store is not initialized
        if (!_userStore.IsInitialized)
            return false;
        // Even if the user store is there to check,
        // checking any further is meaningless if the guild (server) is not available
        if (!_guildAvailable)
            return false;

        // If the user can be found in the store, he's on the server
        return _userStore.TryGetUser(userId, out _);
    }

    private async Task CheckForSpam(SocketMessage userMessage)
    {
        // If the message was received on a direct message channel, it's never spam
        if (userMessage.Channel is IPrivateChannel)
            return;
            
        var checkResult = _spamGuard.CheckForSpam((DiscordUserId)userMessage.Author.Id,
                                                  (DiscordChannelId)userMessage.Channel.Id,
                                                  userMessage.Content,
                                                  userMessage.Attachments?.Count ?? 0);
        if (checkResult == SpamCheckResult.NoSpam)
            return;

        var g = GetGuild();
        var leaderRole = GetRoleByName(Constants.RoleNames.LeaderRoleName);
        var officerRole = GetRoleByName(Constants.RoleNames.OfficerRoleName);

        switch (checkResult)
        {
            case SpamCheckResult.SoftLimitHit:
            {
                var embedBuilder = new EmbedBuilder()
                                  .WithColor(Color.DarkPurple)
                                  .WithTitle("Spam warning")
                                  .WithDescription("Please refrain from further spamming in this channel.");
                var embed = embedBuilder.Build();
                await userMessage.Channel.SendMessageAsync($"{userMessage.Author.Mention} - {leaderRole.Mention} and {officerRole.Mention} have been notified.",
                                                           embed: embed);
                return;
            }
            case SpamCheckResult.BetweenSoftAndHardLimit:
            {
                return;
            }
            case SpamCheckResult.HardLimitHit:
            {
                var guildUser = g.GetUser(userMessage.Author.Id);
                try
                {
                    await guildUser.KickAsync("Excessive spam.", RequestOptions.Default);
                }
                catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
                {
                    await LogToDiscordInternal($"{leaderRole.Mention}, {officerRole.Mention}: Failed to kick user {guildUser.Mention}, because the bot is not permitted to kick a user with a higher rank.");
                    return;
                }
                catch (Exception e)
                {
                    await LogToDiscordInternal($"{leaderRole.Mention}, {officerRole.Mention}: Failed to kick user {guildUser.Mention} due to an unexpected error: {e.Message}");
                    return;
                }

                await LogToDiscordInternal($"{leaderRole.Mention}, {officerRole.Mention}: Kicked user {guildUser.Mention} from the server due to excessive spam.");
                return;
            }
        }
    }

    private bool CheckIfInteractionShouldBeIgnored(SocketInteraction interaction)
    {
        var isStopIgnoreInteraction = interaction is SocketSlashCommand { CommandName: "ignore" } socketSlashCommand
                                   && socketSlashCommand.Data.Options.FirstOrDefault()?.Name == "stop";

        if (isStopIgnoreInteraction || !_ignoreGuard.ShouldIgnore((DiscordUserId)interaction.User.Id))
            return false;

        // If ignore is enabled, we don't reply with any message,
        // as the debug instance of the bot is expected to handle the response.
        return true;
    }

    private static Role SocketRoleToRole(IEnumerable<SocketRole> roles)
    {
        var r = Role.NoRole;
        foreach (var socketRole in roles)
        {
            if (_roleMapping.TryGetValue(socketRole.Name, out var role))
                r |= role;
        }

        return r;
    }

    private static string ConcatRoleNames(IEnumerable<SocketRole> roles)
    {
        return string.Join(", ",
                           roles.Where(m => m.Name != "@everyone" && !m.Name.StartsWith(GroupRolePrefix))
                                .Select(m => TrimRoleName(m.Name))
                                .OrderBy(m => m)
                                .Select(m => $"`{m}`"));

    }

    private async Task LogToDiscordInternal(string? message)
    {
        if (message == null)
            return;

        var g = GetGuild();
        var lc = g.GetTextChannel(_dynamicConfiguration.DiscordMapping["LoggingChannelId"]);
        if (lc == null)
        {
            // Guild or channel can be null because the guild is currently unavailable.
            // In this case, store the messages in a queue and send them later.
            // Otherwise throw.
            if (_guildAvailable)
                throw new ChannelNotFoundException((DiscordChannelId)_dynamicConfiguration.DiscordMapping["LoggingChannelId"]);
            _pendingMessages.Enqueue(message);
            return;
        }

        await lc.SendMessageAsync(message);
    }

    private SocketGuildUser GetGuildUserById(DiscordUserId userId)
    {
        var g = GetGuild();
        return g.GetUser((ulong)userId) ?? throw new InvalidOperationException("User not found.");
    }
    
    private IRole GetRoleByName(string name)
    {
        return GetGuild().Roles.SingleOrDefault(m => m.Name == name)
            ?? throw new InvalidOperationException("Role not found.");
    }

    private static LogLevel ToLogLevel(LogSeverity severity) =>
        severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        };

    private void LogInternal(string prefix, LogMessage msg)
    {
        // Locals
        var handled = false;

        static string FormatLogMessage(string m, Exception? e)
        {
            if (e != null)
                return $"{m}: {e}";
            return e?.ToString() ?? m;
        }

        // Handle special cases
        if (msg.Exception?.InnerException is WebSocketClosedException { CloseCode: 1001 } webSocketClosedException)
        {
            // In case of WebSocketClosedException, check for expected behavior, give the exception more meaning and log only the inner exception data
            _logger.Log(ToLogLevel(msg.Severity), 0, prefix + $"The server sent close 1001 [Going away]: {(string.IsNullOrWhiteSpace(webSocketClosedException.Reason) ? "<no further reason specified>" : webSocketClosedException.Reason)}", null, FormatLogMessage);
            handled = true;
        }

        if (msg.Exception is GatewayReconnectException gatewayReconnectException
         && gatewayReconnectException.Message.Contains("Server missed last heartbeat"))
        {
            // In case of a GatewayReconnectException, the bot might not reconnect correctly to the Discord backend.
            // We kick of a small delay, and then check for the connection state. If it's not reconnected by then, we'll hard-kill the application
            // and wait for the container to restart.
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(2));
                if (_client.ConnectionState != ConnectionState.Connected)
                {
                    _logger.LogCritical(gatewayReconnectException, "Unable to reconnect to Discord backend after last "
                                                                 + $"{nameof(GatewayReconnectException)} and missing last heartbeat. "
                                                                 + "Application will shut down in 10 seconds ...");
                    // Give the logger some time to log the message
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    // Finally kill the process to start over
                    ApplicationLifecycle.End();
                }
            }).ConfigureAwait(false);
        }

        // If the log message has been handled, don't call the default log
        if (handled) return;

        // Default log
        _logger.Log(ToLogLevel(msg.Severity), 0, prefix + msg.Message, msg.Exception, FormatLogMessage);
    }

    private bool CanBotModifyUser(DiscordUserId userId)
    {
        var gu = GetGuildUserById(userId);
        return CanBotModifyUser(gu);
    }

    private bool CanBotModifyUser(SocketGuildUser user)
    {
        var botUser = GetGuildUserById((DiscordUserId)_client.CurrentUser.Id);
        return botUser.Roles.Max(m => m.Position) > user.Roles.Max(m => m.Position);
    }

    private async Task UpdateGuildUserRoles(DiscordUserId userId, Role oldRoles, Role newRoles)
    {
        var result = _discordUserEventHandler.HandleRolesChanged(userId, oldRoles, newRoles);
        if (result.IsPromotion)
        {
            var charactersValid = await VerifyUsernameCharacters();
            if (charactersValid)
                await AnnouncePromotion();
        }

        async Task<bool> VerifyUsernameCharacters()
        {
            try
            {
                var g = GetGuild();
                var gu = GetGuildUserById(userId);
                var username = gu.Username;
                var validCharacters = new Microsoft.AspNetCore.Identity.UserOptions().AllowedUserNameCharacters;
                var valid = username.All(character => validCharacters.Contains(character));
                if (valid)
                    return true;

                // Get channels for notifications
                var privateChannel = await gu.CreateDMChannelAsync();
                var comCoordinatorChannel = g.GetTextChannel(_dynamicConfiguration.DiscordMapping["ComCoordinatorChannelId"]);

                // Prepare messages
                var infoMessage = $"User `{gu.Username}#{gu.DiscriminatorValue}` " +
                                  $"could not be promoted to **Trial Member** because of invalid characters in his username (`{gu.Username}`).";
                var infoMessageWithHereMentionAndInvalidCharacters =
                    "@here - " + infoMessage + $"Only the following characters are allowed: `{validCharacters}`";
                var privateNotificationMessage =
                    $"Hello `{gu.Username}#{gu.DiscriminatorValue}` - your promotion to the rank **Trial Member** failed " +
                    $"because your username (`{gu.Username}`) contains invalid characters. Only the following characters are allowed: " +
                    $"`{validCharacters}`";

                // Send notifications and log message
                await LogToDiscordInternal(infoMessage);
                await comCoordinatorChannel.SendMessageAsync(infoMessageWithHereMentionAndInvalidCharacters);
                await privateChannel.SendMessageAsync(privateNotificationMessage);

                var trialMemberRole = GetRoleByName(nameof(Role.TrialMember));
                await gu.RemoveRoleAsync(trialMemberRole);

                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to verify username characters of user {DiscordUserId}.", userId);
                return true;
            }
        }

        async Task AnnouncePromotion()
        {
            try
            {
                // Log promotion
                await LogToDiscordInternal(result.LogMessage);

                // Announce promotion
                var textChannel = GetGuild().GetTextChannel(_dynamicConfiguration.DiscordMapping["PromotionAnnouncementChannelId"]);
                if (textChannel != null && result.AnnouncementData != null)
                {
                    var embed = result.AnnouncementData.ToEmbed();
                    await textChannel.SendMessageAsync(embed: embed);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Promotion announcement for user {DiscordUserId} failed.", userId);
            }
        }
    }

    private async Task VerifyRoles(DiscordUserId discordUserId,
                                   bool isGuildMember,
                                   IEnumerable<SocketRole> previousRoles,
                                   IEnumerable<SocketRole> currentRoles)
    {
        if (isGuildMember)
            return;

        var newRoles = currentRoles.Except(previousRoles).ToArray();
        if (newRoles.Length == 0)
            return;

        var invalidRoles = _gameRoleProvider.Games.Where(m => m.AvailableRoles is { Count: > 0 })
                                            .Join(newRoles,
                                                  game => game.PrimaryGameDiscordRoleId,
                                                  role => (DiscordRoleId)role.Id,
                                                  (_, role) => role.Name)
                                            .ToArray();
        if (invalidRoles.Length == 0)
            return;
        
        var gu = GetGuildUserById(discordUserId);
        var leaderRole = GetRoleByName(Constants.RoleNames.LeaderRoleName);
        var officerRole = GetRoleByName(Constants.RoleNames.OfficerRoleName);
        foreach (var invalidRole in invalidRoles)
        {
            await LogToDiscordInternal($"{leaderRole?.Mention} {officerRole?.Mention}: User `{gu.Username}#{gu.DiscriminatorValue}` " +
                                       $"is no guild member but was assigned the role `{invalidRole}`. " +
                                       "Please verify the correctness of this role assignment.");
        }
    }

    private async Task ApplyGroupRoles(DiscordUserId discordUserId,
                                       IReadOnlyCollection<SocketRole> previousRoles,
                                       IReadOnlyCollection<SocketRole> currentRoles)
    {
        if (!CanBotModifyUser(discordUserId))
            return;

        var rolesRemoved = previousRoles.Except(currentRoles)
                                        .Any(m => !m.Name.StartsWith(GroupRolePrefix));
        var rolesAdded = currentRoles.Except(previousRoles)
                                     .Any(m => !m.Name.StartsWith(GroupRolePrefix));
        if (!rolesRemoved && !rolesAdded)
            return;

        var g = GetGuild();
        var groupRoles = g.Roles
                          .Where(m => m.Name.StartsWith(GroupRolePrefix))
                          .OrderBy(m => m.Position)
                          .ToArray();
        var groupRolesToAdd = new List<SocketRole>();
        var groupRolesToRemove = new List<SocketRole>();
        var previousGroupRolePosition = 0;
        foreach (var groupRole in groupRoles)
        {
            var hasRoleInGroup = currentRoles.Any(m => m.Position > previousGroupRolePosition && m.Position < groupRole.Position);
            if (!currentRoles.Contains(groupRole) && hasRoleInGroup)
                groupRolesToAdd.Add(groupRole);
            else if (currentRoles.Contains(groupRole) && !hasRoleInGroup)
                groupRolesToRemove.Add(groupRole);
            previousGroupRolePosition = groupRole.Position;
        }

        var gu = GetGuildUserById(discordUserId);
        try
        {
            if (groupRolesToAdd.Any())
            {
                _logger.LogInformation("Adding user {Username} ({DiscordUserId}) to these group roles: {Roles}",
                                       gu.Username,
                                       gu.Id,
                                       string.Join(", ", groupRolesToAdd.Select(m => $"{TrimRoleName(m.Name)} ({m.Id})")));
                await gu.AddRolesAsync(groupRolesToAdd);
            }

            if (groupRolesToRemove.Any())
            {
                _logger.LogInformation("Removing user {Username} ({DiscordUserId}) from these group roles: {Roles}",
                                       gu.Username,
                                       gu.Id,
                                       string.Join(", ", groupRolesToAdd.Select(m => $"{TrimRoleName(m.Name)} ({m.Id})")));
                await gu.RemoveRolesAsync(groupRolesToRemove);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add or remove user {Username} ({DiscordUserId}) to or from group roles.",
                             gu.Username,
                             gu.Id);
        }
    }

    private static string TrimRoleName(string roleName)
    {
        var result = roleName.Trim();
        while (result.Contains('\u2063'))
            result = result.Replace("\u2063", string.Empty);
        while (result.Contains('\u2002'))
            result = result.Replace("\u2002", string.Empty);

        return result;
    }

    private static bool IsOnline(IPresence gu) => gu.Status != UserStatus.Offline && gu.Status != UserStatus.Invisible;
    
    private SocketGuildUser[] GetGuildMembersWithRoles(DiscordRoleId[]? roleIds,
                                                       DiscordRoleId[]? roleIdsToExclude)
    {
        var g = GetGuild();
        return g.Users
                .Where(m => _userStore.TryGetUser((DiscordUserId) m.Id, out var user)
                         && user!.IsGuildMember
                         && (roleIds == null || roleIds.Intersect(m.Roles.Select(r => (DiscordRoleId)r.Id)).Count() == roleIds.Length)
                         && (roleIdsToExclude == null || !roleIdsToExclude.Intersect(m.Roles.Select(r => (DiscordRoleId)r.Id)).Any()))
                .ToArray();
    }

    private SocketGuildUser[] GetGuildMembersWithAnyGivenRole(SocketGuild guild,
                                                              ulong[]? roleIds)
    {
        return guild.Users
                    .Where(m => _userStore.TryGetUser((DiscordUserId) m.Id, out var user)
                             && user!.IsGuildMember
                             && (roleIds == null || roleIds.Intersect(m.Roles.Select(r => r.Id)).Any()))
                    .ToArray();
    }

    private static MessageComponent ToMessageComponent(IEnumerable<ActionComponent> components)
    {
        var builder = new ComponentBuilder();

        foreach (var component in components)
        {
            if (component is ButtonComponent button)
            {
                var buttonBuilder = new ButtonBuilder()
                                   .WithCustomId(button.CustomId)
                                   .WithLabel(button.Label)
                                   .WithStyle((ButtonStyle)button.Style);
                builder.WithButton(buttonBuilder, button.ActionRowNumber);
            }
            else if (component is SelectMenuComponent selectMenu)
            {
                var menuBuilder = new SelectMenuBuilder()
                                 .WithCustomId(selectMenu.CustomId)
                                 .WithPlaceholder(selectMenu.Placeholder)
                                 .WithMinValues(0)
                                 .WithMaxValues(selectMenu.AllowMultiple ? selectMenu.Options.Count : 1);
                foreach (var (optionKey, label) in selectMenu.Options)
                    menuBuilder.AddOption(label, optionKey);
                builder.WithSelectMenu(menuBuilder, selectMenu.ActionRowNumber);
            }
        }

        return builder.Build();
    }

    private static async Task<TResult> WithRetry<TResult>(Func<Task<TResult>> act,
                                                          Action<Exception> handleError,
                                                          TResult fallbackResult)
    {
        var tries = 0;
        while (true)
        {
            tries++;
            try
            {
                return await act();
            }
            catch (Exception e)
            {
                handleError(e);
                if (tries >= 5)
                    return fallbackResult;

                // Retry in 500 ms.
                await Task.Delay(500);
            }
        }
    }

    bool IDiscordAccess.IsConnected => _client.ConnectionState == ConnectionState.Connected;

    bool IDiscordAccess.IsGuildAvailable => _guildAvailable;

    async Task IDiscordAccess.Connect(Func<Task> connectedHandler, Func<Task> disconnectedHandler)
    {
        if (connectedHandler == null)
            throw new ArgumentNullException(nameof(connectedHandler));
        if (disconnectedHandler == null)
            throw new ArgumentNullException(nameof(disconnectedHandler));

        Func<Task> ClientConnected()
        {
            return () =>
            {
                _logger.LogDebug("Bot client connected. Running post-connected logic ...");
                // Fire & Forget
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await connectedHandler();
                        _client.InteractionCreated += Client_InteractionCreated;
                        _client.ButtonExecuted += Client_ButtonExecuted;
                        _client.SelectMenuExecuted += Client_SelectMenuExecuted;
                        _client.MessageReceived += Client_MessageReceived;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to run post-connected logic.");
                    }
                }).ConfigureAwait(false);
                // Return immediately
                return Task.CompletedTask;
            };
        }

        Func<Exception, Task> ClientDisconnected()
        {
            return _ =>
            {
                // Fire & Forget
                Task.Run(async () =>
                {
                    _client.MessageReceived -= Client_MessageReceived;
                    _client.InteractionCreated -= Client_InteractionCreated;
                    _client.ButtonExecuted -= Client_ButtonExecuted;
                    _client.SelectMenuExecuted -= Client_SelectMenuExecuted;
                    await disconnectedHandler();
                }).ConfigureAwait(false);
                // Return immediately
                return Task.CompletedTask;
            };
        }
            
        try
        {
            _logger.LogInformation("Establishing initial Discord connection...");
            _client.Connected -= ClientConnected();
            _client.Connected += ClientConnected();
            _client.Disconnected -= ClientDisconnected();
            _client.Disconnected += ClientDisconnected();
            _client.GuildAvailable -= Client_GuildAvailable;
            _client.GuildAvailable += Client_GuildAvailable;
            _client.GuildUnavailable -= Client_GuildUnavailable;
            _client.GuildUnavailable += Client_GuildUnavailable;
            _client.UserJoined -= Client_UserJoined;
            _client.UserJoined += Client_UserJoined;
            _client.UserLeft -= Client_UserLeft;
            _client.UserLeft += Client_UserLeft;
            _client.PresenceUpdated -= Client_PresenceUpdated;
            _client.PresenceUpdated += Client_PresenceUpdated;
            _client.GuildMemberUpdated -= Client_GuildMemberUpdated;
            _client.GuildMemberUpdated += Client_GuildMemberUpdated;

            _logger.LogInformation("Performing login...");
            await _client.LoginAsync(TokenType.Bot, _rootSettings.DiscordBotToken);
            _logger.LogInformation("Starting client...");
            await _client.StartAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error while connecting to Discord.");
        }
    }

    async Task IDiscordAccess.SetCurrentGame(string gameName)
    {
        await _client.SetGameAsync(gameName);
    }

    async Task IDiscordAccess.LogToDiscord(string message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Log message must have content.", nameof(message));

        await LogToDiscordInternal(message);
    }

    bool IDiscordAccess.IsUserOnline(DiscordUserId userId)
    {
        var gu = GetGuildUserById(userId);
        return IsOnline(gu);
    }

    Dictionary<DiscordUserId, string> IDiscordAccess.GetUserNames(IEnumerable<DiscordUserId> userIds) => userIds.Select(GetGuildUserById).ToDictionary(gu => (DiscordUserId)gu.Id, gu => gu.Username);

    async Task<bool> IDiscordAccess.TryAddNonMemberRole(DiscordUserId userId,
                                                        Role targetRole)
    {
        var roleDisplayName = _roleMapping.Single(m => m.Value == targetRole).Key;
        var role = GetRoleByName(roleDisplayName);
        var gu = GetGuildUserById(userId);
        if (gu.Roles.Any(m => m.Id == role.Id))
            return false;

        return await WithRetry(async () =>
                               {
                                   await gu.AddRoleAsync(role);
                                   return true;
                               },
                               e => _logger.LogError(e, "Failed to set game role '{Role}' for user {UserId}.", role.Name, userId),
                               false);
    }

    async Task<(bool Success, string RoleName)> IDiscordAccess.TryAddNonMemberRole(DiscordUserId userId,
                                                                                   DiscordRoleId targetRole)
    {
        var g = GetGuild();
        var role = g.GetRole((ulong)targetRole);
        var gu = GetGuildUserById(userId);
        if (gu.Roles.Any(m => m.Id == role.Id))
            return (false, role.Name);

        return await WithRetry(async () =>
                               {
                                   await gu.AddRoleAsync(role);
                                   return (true, role.Name);
                               },
                               e => _logger.LogError(e, "Failed to set game role '{Role}' for user {UserId}.", role.Name, userId),
                               (false, role.Name));
    }

    async Task<bool> IDiscordAccess.TryRevokeNonMemberRole(DiscordUserId userId,
                                                           Role targetRole)
    {
        var roleDisplayName = _roleMapping.Single(m => m.Value == targetRole).Key;
        var role = GetRoleByName(roleDisplayName);
        var gu = GetGuildUserById(userId);
        if (gu.Roles.All(m => m.Id != role.Id))
            return false;

        return await WithRetry(async () =>
                               {
                                   await gu.RemoveRoleAsync(role);
                                   return true;
                               },
                               e => _logger.LogError(e, "Failed to revoke game role '{Role}' for user {UserId}.", role.Name, userId),
                               false);
    }

    async Task<(bool Success, string RoleName)> IDiscordAccess.TryRevokeNonMemberRole(DiscordUserId userId,
                                                                                      DiscordRoleId targetRole)
    {
        var g = GetGuild();
        var role = g.GetRole((ulong)targetRole);
        var gu = GetGuildUserById(userId);
        if (gu.Roles.All(m => m.Id != role.Id))
            return (false, role.Name);

        return await WithRetry(async () =>
                               {
                                   await gu.RemoveRoleAsync(role);
                                   return (true, role.Name);
                               },
                               e => _logger.LogError(e, "Failed to revoke game role '{Role}' for user {UserId}.", role.Name, userId),
                               (false, role.Name));
    }

    async Task<bool> IDiscordAccess.TryAssignRoleAsync(DiscordUserId userId,
                                                       DiscordRoleId roleId)
    {
        // Get role
        var g = GetGuild();
        var role = g.GetRole((ulong)roleId);

        // Get user and check if the role is currently assigned.
        var gu = GetGuildUserById(userId);
        if (gu.Roles.Any(m => m.Id == role.Id))
            return false; // If it's assigned, we cannot assign it.

        return await WithRetry(async () =>
                               {
                                   // If the role isn't currently assigned, assign it to the user.
                                   await gu.AddRoleAsync(role);
                                   return true;
                               },
                               e => _logger.LogError(e, "Failed to add '{Role}' to UserId {DiscordUserId}.", role.Name, userId),
                               false);
    }

    async Task<bool> IDiscordAccess.TryRevokeGameRole(DiscordUserId userId,
                                                      DiscordRoleId roleId)
    {
        // Get role
        var g = GetGuild();
        var role = g.GetRole((ulong)roleId);

        // Get user and check if the role is currently assigned.
        var gu = GetGuildUserById(userId);
        if (gu.Roles.All(m => m.Id != role.Id))
            return false; // If isn't assigned, we cannot revoke it.

        return await WithRetry(async () =>
                               {
                                   // If the role is currently assigned, revoke it from the user.
                                   await gu.RemoveRoleAsync(role);
                                   return true;
                               },
                               e => _logger.LogError(e, "Failed to revoke '{Role}' for UserId {DiscordUserId}.", role.Name, userId),
                               false);
    }

    bool IDiscordAccess.CanManageRolesForUser(DiscordUserId userId)
    {
        return CanBotModifyUser(userId);
    }

    string IDiscordAccess.GetRoleMention(string roleName)
    {
        return GetRoleByName(roleName).Mention;
    }

    async Task<TextMessage[]> IDiscordAccess.GetBotMessagesInChannel(DiscordChannelId channelId)
    {
        var result = new List<TextMessage>();
        var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelId);
        var messageCollection = channel.GetMessagesAsync();
        var enumerator = messageCollection.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync())
        {
            if (enumerator.Current == null)
                continue;

            result.AddRange(enumerator.Current.Where(m => m.Author.Id == _client.CurrentUser.Id)
                                      .Select(message => new TextMessage(message.Content,
                                                                         GetCustomIdsAndOptions(message.Components))));
        }

        result.Reverse();
        return result.ToArray();

        static Dictionary<string, Dictionary<string, string>?> GetCustomIdsAndOptions(IEnumerable<IMessageComponent> components)
        {
            var customIds = new Dictionary<string, Dictionary<string, string>?>();

            foreach (var messageComponent in components)
            {
                switch (messageComponent.Type)
                {
                    case ComponentType.ActionRow:
                        var nested = GetCustomIdsAndOptions(((ActionRowComponent)messageComponent).Components);
                        foreach (var n in nested)
                            customIds[n.Key] = n.Value;
                        break;
                    case ComponentType.Button:
                        customIds.Add(messageComponent.CustomId, null);
                        break;
                    case ComponentType.SelectMenu:
                        if (messageComponent is global::Discord.SelectMenuComponent selectMenuComponent)
                            customIds.Add(messageComponent.CustomId, selectMenuComponent.Options.ToDictionary(m => m.Value, m => m.Label));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(messageComponent), "Unknown component type.");
                }
            }

            return customIds;
        }
    }

    async Task IDiscordAccess.DeleteBotMessagesInChannel(DiscordChannelId channelId)
    {
        var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelId);
        var messagesToDelete = new List<IMessage>();
        var messageCollection = channel.GetMessagesAsync();
        var enumerator = messageCollection.GetAsyncEnumerator();
        _logger.LogTrace("Fetching messages to delete in channel {ChannelId} ...", channelId);
        while (await enumerator.MoveNextAsync())
        {
            if (enumerator.Current == null) continue;
            messagesToDelete.AddRange(enumerator.Current.Where(m => m.Author.Id == _client.CurrentUser.Id));
        }

        var current = 0;
        await messagesToDelete.PerformBulkOperation(async message =>
        {
            current++;
            try
            {
                _logger.LogTrace("Channel {ChannelId}: Deleting message {CurrentMessageNumber}/{TotalMessagesToDelete} with Id {MessageId}.",
                                 channelId, current, messagesToDelete.Count, message.Id);
                await message.DeleteAsync();
                _logger.LogTrace("Channel {ChannelId}: Deleted message {CurrentMessageNumber}/{TotalMessagesToDelete} with Id {MessageId}.",
                                 channelId, current, messagesToDelete.Count, message.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete message with Id {MessageId} in channel {ChannelId}.", message.Id, channelId);
            }
        });
    }

    async Task IDiscordAccess.DeleteBotMessageInChannelAsync(DiscordChannelId channelId, ulong messageId)
    {
        var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelId);
        var message = await channel.GetMessageAsync(messageId);
        await message.DeleteAsync();
    }

    async Task<ulong[]> IDiscordAccess.CreateBotMessagesInChannelAsync(DiscordChannelId channelId, string[] messages)
    {
        var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelId);
        var result = new List<ulong>();

        var current = 0;
        await messages.PerformBulkOperation(async message =>
        {
            current++;
            _logger.LogTrace("Channel {ChannelId}: Creating message {CurrentMessageNumber}/{TotalMessages} ...",
                             channelId, current, messages.Length);
            var createdMessage = await channel.SendMessageAsync(message);
            result.Add(createdMessage.Id);
            _logger.LogTrace("Channel {ChannelId}: Created message {CurrentMessageNumber}/{TotalMessages} with Id {CreatedMessageId}.",
                             channelId, current, messages.Length, createdMessage.Id);
        });

        return result.ToArray();
    }

    async Task IDiscordAccess.CreateBotMessagesInChannelAsync(DiscordChannelId channelId,
                                                              (string Content, ActionComponent[] Components)[] messages)
    {
        if (messages.Any(m => m.Components.OfType<ButtonComponent>().Count() > 25))
            throw new ArgumentOutOfRangeException(nameof(messages), "At least one message contains too many buttons.");
        if (messages.Any(m => m.Components.OfType<ButtonComponent>().GroupBy(b => b.ActionRowNumber).Any(g => g.Count() > 5)))
            throw new ArgumentOutOfRangeException(nameof(messages), "At least one message contains too many buttons in a single row.");
        if (messages.Any(m => m.Components.OfType<SelectMenuComponent>().Count() > 5))
            throw new ArgumentOutOfRangeException(nameof(messages), "At least one message contains too many select menus.");
        if (messages.Any(m => m.Components.OfType<SelectMenuComponent>().GroupBy(b => b.ActionRowNumber).Any(g => g.Count() > 1)))
            throw new ArgumentOutOfRangeException(nameof(messages), "At least one message contains more than one select menu in a single row.");
        if (messages.Any(m => m.Components.OfType<SelectMenuComponent>().Any(c => c.Options.Count > 25)))
            throw new ArgumentOutOfRangeException(nameof(messages), "At least one message contains a select menus with too many options.");

        var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelId);

        var current = 0;
        await messages.PerformBulkOperation(async message =>
        {
            current++;
            _logger.LogTrace("Channel {ChannelId}: Creating message {CurrentMessageNumber}/{TotalMessages} ...", channelId, current, messages.Length);
            var createdMessage = await channel.SendMessageAsync(message.Content, components: ToMessageComponent(message.Components));
            _logger.LogTrace("Channel {ChannelId}: Created message {CurrentMessageNumber}/{TotalMessages} with Id {CreatedMessageId}.",
                             channelId, current, messages.Length, createdMessage.Id);
        });
    }

    async Task IDiscordAccess.CreateBotMessageInWelcomeChannel(string message)
    {
        var g = GetGuild();
        await g.SystemChannel.SendMessageAsync(message);
    }

    int IDiscordAccess.CountGuildMembersWithRoles(DiscordRoleId[] roleIds)
    {
        var guildMembers = GetGuildMembersWithRoles(roleIds, null);
        return guildMembers.Length;
    }

    int IDiscordAccess.CountGuildMembersWithRoles(DiscordRoleId[]? roleIds,
                                                  DiscordRoleId[] roleIdsToExclude)
    {
        var guildMembers = GetGuildMembersWithRoles(roleIds, roleIdsToExclude);
        return guildMembers.Length;
    }

    int IDiscordAccess.CountGuildMembersWithRoles(string[] roleNames)
    {
        var roleIds = new List<DiscordRoleId>();
        foreach (var roleName in roleNames)
        {
            roleIds.Add((DiscordRoleId)GetRoleByName(roleName).Id);
        }
        var guildMembers = GetGuildMembersWithRoles(roleIds.ToArray(), null);
        return guildMembers.Length;
    }

    string IDiscordAccess.GetCurrentDisplayName(DiscordUserId userId)
    {
        var gu = GetGuildUserById(userId);
        return string.IsNullOrWhiteSpace(gu.Nickname) ? gu.Username : gu.Nickname;
    }

    string IDiscordAccess.GetChannelLocationAndName(DiscordChannelId discordChannelId)
    {
        var g = GetGuild();
        var channel = g.TextChannels.Single(m => m.Id == (ulong) discordChannelId);
        return $"/{channel.Category.Name}/{channel.Name}";
    }

    async Task<(DiscordChannelId VoiceChannelId, string? Error)> IDiscordAccess.CreateVoiceChannel(DiscordChannelId voiceChannelsCategoryId,
                                                                                                   string name,
                                                                                                   int maxUsers)
    {
        var g = GetGuild();
        try
        {
            if (g.VoiceChannels.Any(m => m.Name == name))
                return ((DiscordChannelId)0, "Voice channel with same name already exists.");

            var voiceChannel = await g.CreateVoiceChannelAsync(name,
                                                               properties =>
                                                               {
                                                                   properties.UserLimit = maxUsers;
                                                                   properties.CategoryId = (ulong)voiceChannelsCategoryId;
                                                               });
            return ((DiscordChannelId)voiceChannel.Id, null);
        }
        catch (Exception e)
        {
            return ((DiscordChannelId)0, e.Message);
        }
    }

    async Task IDiscordAccess.ReorderChannelsAsync(DiscordChannelId[] channelIds,
                                                   DiscordChannelId positionAboveChannelId)
    {
        var g = GetGuild();
        try
        {
            var baseChannel = g.GetChannel((ulong)positionAboveChannelId) as INestedChannel;
            if (baseChannel?.CategoryId == null)
            {
                _logger.LogWarning("Trying to reorder channels, " +
                                   "but the given base channel ({ChannelId}) to position the channels above is not a nested channel.",
                                   positionAboveChannelId);
                return;
            }

            if (channelIds.Length == 0)
                return;

            var finalList = new List<ReorderChannelProperties>();
            var baseChannelCategory = g.GetCategoryChannel(baseChannel.CategoryId.Value);
            var channelsInCategory = baseChannelCategory.Channels
                                                        .Where(m => !channelIds.Contains((DiscordChannelId)m.Id))
                                                        .OrderBy(m => m.Position)
                                                        .ToArray();

            var position = 1;
            foreach (var channel in channelsInCategory)
            {
                if ((DiscordChannelId)channel.Id == positionAboveChannelId)
                {
                    foreach (var channelId in channelIds)
                    {
                        finalList.Add(new ReorderChannelProperties((ulong)channelId, position));
                        position++;
                    }
                    finalList.Add(new ReorderChannelProperties(channel.Id, position));
                }
                else
                {
                    finalList.Add(new ReorderChannelProperties(channel.Id, position));
                }
                position++;
            }
                
            await g.ReorderChannelsAsync(finalList);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to reorder channels.");
        }
    }

    async Task IDiscordAccess.DeleteVoiceChannel(DiscordChannelId voiceChannelId)
    {
        var g = GetGuild();
        var voiceChannel = g.GetVoiceChannel((ulong)voiceChannelId);
        if (voiceChannel == null)
            return;

        await voiceChannel.DeleteAsync();
    }

    string IDiscordAccess.GetAvatarId(DiscordUserId userId)
    {
        var gu = GetGuildUserById(userId);
        return gu.AvatarId;
    }

    UserModel[] IDiscordAccess.GetUsersInRoles(string[] allowedRoles)
    {
        var g = GetGuild();
        var allowedRoleIds = allowedRoles.Select(GetRoleByName)
                                         .Select(m => m.Id)
                                         .ToArray();
        var users = GetGuildMembersWithAnyGivenRole(g, allowedRoleIds);
        return users.Select(m => new UserModel
                     {
                         DiscordUserId = (DiscordUserId)m.Id,
                         Username = m.Username,
                         Discriminator = (short)m.DiscriminatorValue,
                         Nickname = m.Nickname,
                         AvatarId = m.AvatarId,
                         Roles = m.Roles
                                  .Where(r => allowedRoleIds.Contains(r.Id))
                                  .Select(r => r.Name)
                                  .ToArray()
                     })
                    .ToArray();
    }

    string IDiscordAccess.GetLeadershipMention()
    {
        var leaderRole = GetRoleByName(Constants.RoleNames.LeaderRoleName);
        var officerRole = GetRoleByName(Constants.RoleNames.OfficerRoleName);
        return $"{leaderRole.Mention} {officerRole.Mention}";
    }

    async Task IDiscordAccess.SendUnitsNotificationAsync(EmbedData embedData)
    {
        try
        {
            var g = GetGuild();
            var channel = g.GetTextChannel(_dynamicConfiguration.DiscordMapping["UnitsNotificationsChannelId"]);
            var embed = embedData.ToEmbed();
            await channel.SendMessageAsync(null, false, embed);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send UNITS notification.");
        }
    }

    public async Task SendUnitsNotificationAsync(EmbedData embedData,
                                                 DiscordUserId[] usersToNotify)
    {
        try
        {
            var g = GetGuild();
            var channel = g.GetTextChannel(_dynamicConfiguration.DiscordMapping["UnitsNotificationsChannelId"]);
            var embed = embedData.ToEmbed();
            var notifications = new List<string>();
            var allMentions = new Queue<string>(usersToNotify.Select(m => m.ToMention() + " "));
            var totalLength = allMentions.Sum(m => m.Length);
            var totalMessagesRequired = (int)Math.Ceiling(totalLength / 1500d);
            if (totalMessagesRequired == 1)
            {
                notifications.Add(string.Join(string.Empty, allMentions));
            }
            else
            {
                string? addToNext = null;
                for (var i = 0; i < totalMessagesRequired; i++)
                {
                    var sb = new StringBuilder();
                    if (addToNext != null)
                    {
                        sb.Append(addToNext);
                        addToNext = null;
                    }

                    while (allMentions.TryDequeue(out var nextMention))
                    {
                        if (sb.Length + nextMention.Length > 1500)
                        {
                            addToNext = nextMention;
                            break;
                        }

                        sb.Append(nextMention);
                    }
                    notifications.Add(sb.ToString());
                }
            }
            await channel.SendMessageAsync(notifications[0], false, embed);
            if (notifications.Count > 1)
            {
                foreach (var notification in notifications.Skip(1))
                {
                    await Task.Delay(500);
                    await channel.SendMessageAsync(notification, false, embed);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to send UNITS notification with mentions.");
        }
    }

    Dictionary<string, List<DiscordUserId>> IDiscordAccess.GetUsersInVoiceChannels(string[] voiceChannelIds)
    {
        var g = GetGuild();
        var result = new Dictionary<string, List<DiscordUserId>>();
        foreach (var voiceChannelIdStr in voiceChannelIds)
        {
            if (!ulong.TryParse(voiceChannelIdStr, out var voiceChannelId))
                continue;

            var voiceChannel = g.GetVoiceChannel(voiceChannelId);
            var userIds = voiceChannel.Users
                                      .Select(m => (DiscordUserId) m.Id)
                                      .ToList();
            if (userIds.Any())
                result.Add(voiceChannelIdStr, userIds);
        }

        return result;
    }

    List<DiscordUserId> IDiscordAccess.GetUsersIdsInRole(DiscordRoleId roleId)
    {
        var g = GetGuild();
        var role = g.GetRole((ulong)roleId);
        return role?.Members
                    .Select(roleMember => (DiscordUserId)roleMember.Id)
                    .ToList()
            ?? new List<DiscordUserId>();
    }

    void IDiscordAccess.EnsureDisplayNamesAreSet(IEnumerable<AvailableGame> games)
    {
        var g = GetGuild();
        foreach (var game in games)
        {
            var discordRole = g.GetRole((ulong)game.PrimaryGameDiscordRoleId);
            game.DisplayName = discordRole?.Name ?? "< Unknown game >";
        }
    }

    void IDiscordAccess.EnsureDisplayNamesAreSet(IEnumerable<AvailableGameRole> gameRoles)
    {
        var g = GetGuild();
        foreach (var role in gameRoles)
        {
            var discordRole = g.GetRole((ulong)role.DiscordRoleId);
            role.DisplayName = discordRole?.Name ?? "< Unknown game role >";
        }
    }

    DiscordRoleId[] IDiscordAccess.GetUserRoles(DiscordUserId userId)
    {
        var gu = GetGuildUserById(userId);
        return gu.Roles.Select(m => (DiscordRoleId)m.Id).ToArray();
    }

    private Task LogClient(LogMessage arg)
    {
        LogInternal($"{nameof(DiscordSocketClient)}: ", arg);
        return Task.CompletedTask;
    }

    private Task LogInteractions(LogMessage arg)
    {
        LogInternal($"{nameof(InteractionService)}: ", arg);
        return Task.CompletedTask;
    }

    private async Task Client_GuildAvailable(SocketGuild guild)
    {
        if (guild.Id != _dynamicConfiguration.DiscordMapping["ValidGuildId"])
            return;

        _guildAvailable = true;
        _logger.LogInformation("Guild '{GuildName}' is available.", guild.Name);
        _ = Task.Run(async () =>
        {
            if (!_userStore.IsInitialized)
            {
                // Initialize user store only once
                await guild.DownloadUsersAsync();
                var allGuildUsers = guild.Users;
                var mappedGuildUsers = allGuildUsers.Select(m => ((DiscordUserId) m.Id,
                                                                  SocketRoleToRole(m.Roles),
                                                                  ConcatRoleNames(m.Roles),
                                                                  m.JoinedAt?.DateTime.ToUniversalTime() ?? User.DefaultJoinedDate))
                                                    .ToArray();
                await _userStore.Initialize(mappedGuildUsers);
            }
                
            if (_gameRoleProvider.Games.Count == 0)
            {
                // Load games only once
                await _gameRoleProvider.LoadAvailableGames();
            }

            // Ensure that static messages exist
            await _staticMessageProvider.EnsureStaticMessagesExist();

            // TODO: Register once per version/the commands change, not every time the bot boots
            // Register interactions only once
            var addedModules = (await _interactionService.AddModulesAsync(typeof(DiscordAccess).Assembly, _serviceProvider)).ToArray();
            _logger.LogInformation("Found {Count} modules in the assembly.", addedModules.Length);
            var registeredCommands = await _interactionService.RegisterCommandsToGuildAsync(guild.Id);
            _logger.LogInformation("Registered {Count} commands with Discord.", registeredCommands.Count);
        }).ConfigureAwait(false);

        while (_pendingMessages.Count > 0)
        {
            var pm = _pendingMessages.Dequeue();
            await LogToDiscordInternal(pm);
        }
    }

    private Task Client_GuildUnavailable(SocketGuild guild)
    {
        if (guild.Id != _dynamicConfiguration.DiscordMapping["ValidGuildId"])
            return Task.CompletedTask;

        _guildAvailable = false;

        return Task.CompletedTask;
    }

    private Task Client_UserJoined(SocketGuildUser guildUser)
    {
        _discordUserEventHandler.HandleJoined((DiscordUserId)guildUser.Id,
                                              SocketRoleToRole(guildUser.Roles),
                                              guildUser.JoinedAt?.Date.ToUniversalTime() ?? User.DefaultJoinedDate);
        return Task.CompletedTask;
    }

    private Task Client_UserLeft(SocketGuild guild, SocketUser user)
    {
        if (guild == null)
            throw new ArgumentNullException(nameof(guild));
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            _discordUserEventHandler.HandleLeft((DiscordUserId)user.Id,
                                                user.Username,
                                                user.DiscriminatorValue);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unhandled exception has been thrown while handling UserLeft event.");
        }

        return Task.CompletedTask;
    }

    private Task Client_PresenceUpdated(SocketUser socketUser, SocketPresence? oldPresence, SocketPresence? newPresence)
    {
        var discordUserId = (DiscordUserId)socketUser.Id;

        // Fire & Forget
        Task.Run(async () =>
        {
            // Handle possible status change
            if (oldPresence is not null
             && newPresence is not null
             && oldPresence.Status != newPresence.Status)
            {
                var wasOnline = IsOnline(oldPresence);
                var isOnline = IsOnline(newPresence);
                await _discordUserEventHandler.HandleStatusChanged(discordUserId, wasOnline, isOnline);
            }
        });
        return Task.CompletedTask;
    }

    private Task Client_GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> oldGuildUser, SocketGuildUser newGuildUser)
    {
        var validGuildId = _dynamicConfiguration.DiscordMapping["ValidGuildId"];
        if (oldGuildUser.Value.Guild.Id != validGuildId)
            return Task.CompletedTask;
        if (newGuildUser.Guild.Id != validGuildId)
            return Task.CompletedTask;
        if (oldGuildUser.Id != newGuildUser.Id)
            return Task.CompletedTask;

        // Fire & Forget
        Task.Run(async () =>
        {
            var discordUserId = (DiscordUserId)newGuildUser.Id;

            // Handle possible role change
            var oldRoles = SocketRoleToRole(oldGuildUser.Value.Roles);
            var newRoles = SocketRoleToRole(newGuildUser.Roles);
            if (oldRoles != newRoles)
            {
                await UpdateGuildUserRoles(discordUserId, oldRoles, newRoles);
            }

            // Handle possible role change, that only should be there for guild members
            if (_userStore.TryGetUser(discordUserId, out var user))
            {
                await VerifyRoles(user!.DiscordUserId,
                                  user.IsGuildMember,
                                  oldGuildUser.Value.Roles,
                                  newGuildUser.Roles);
            }

            // Handle possible role change, that would end up in new or removed group roles
            await ApplyGroupRoles(discordUserId,
                                  oldGuildUser.Value.Roles,
                                  newGuildUser.Roles);

            // Handle possible role changes that need to be persisted in case the user loses his roles by accident.
            var previousRoleNames = ConcatRoleNames(oldGuildUser.Value.Roles);
            var currentRoleNames = ConcatRoleNames(newGuildUser.Roles);
            if (previousRoleNames != currentRoleNames)
            {
                await _discordUserEventHandler.HandleRolesChanged(discordUserId, currentRoleNames);
            }
        });
        return Task.CompletedTask;
    }

    private async Task Client_InteractionCreated(SocketInteraction? interaction)
    {
        if (interaction is null)
            return;

        if (CheckIfInteractionShouldBeIgnored(interaction))
            return;

        try
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to execute command for interaction.");
        }
    }

    private async Task Client_ButtonExecuted(SocketMessageComponent component)
    {
        var response = await _discordUserEventHandler
                          .HandleMessageComponentExecutedAsync((DiscordUserId)component.User.Id,
                                                               component.Data.CustomId,
                                                               null,
                                                               component.Data.Values)
                    ?? "Internal error.";
        await component.RespondAsync(response, ephemeral: true);
    }

    private async Task Client_SelectMenuExecuted(SocketMessageComponent component)
    {
        var availableOptions =
            component.Message
                     .Components
                     .SelectMany(c => c.Components.OfType<global::Discord.SelectMenuComponent>()
                                       .Where(smc => smc.CustomId == component.Data.CustomId)
                                       .SelectMany(smc => smc.Options.Select(smo => smo.Value)))
                     .ToArray();
        var response = await _discordUserEventHandler
                          .HandleMessageComponentExecutedAsync((DiscordUserId)component.User.Id,
                                                               component.Data.CustomId,
                                                               availableOptions,
                                                               component.Data.Values)
                    ?? "Internal error.";
        await component.RespondAsync(response, ephemeral: true);
    }

    private async Task InteractionService_SlashCommandExecuted(SlashCommandInfo commandInfo,
                                                               IInteractionContext ctx,
                                                               IResult interactionResult)
    {
        if (interactionResult.IsSuccess)
            return;

        var builder = new EmbedBuilder();
        switch (interactionResult.Error)
        {
            case InteractionCommandError.UnmetPrecondition:
            {
                builder.WithColor(Color.Red)
                       .WithTitle("Couldn't execute interaction due to unmet precondition")
                       .WithDescription(interactionResult.ErrorReason)
                       .WithDescription(interactionResult.ErrorReason);

                if (interactionResult is PreconditionGroupResult preconditionGroupResult)
                {
                    var results = preconditionGroupResult.Results.ToArray();
                    for (var i = 0; i < results.Length; i++)
                    {
                        builder.AddField($"Error {i + 1}",
                                         results[i].ErrorReason);
                    }
                }

                break;
            }
            case InteractionCommandError.Exception:
                builder.WithColor(Color.DarkRed)
                       .WithTitle("Couldn't execute interaction due to an execution exception. Please contact the developer.")
                       .WithDescription(interactionResult.ErrorReason);
                break;
            default:
                builder.WithColor(Color.DarkOrange)
                       .WithTitle($"{interactionResult.Error ?? InteractionCommandError.Unsuccessful}: Couldn't execute interaction.")
                       .WithDescription(interactionResult.ErrorReason);
                break;
        }

        await ctx.Interaction.RespondAsync(embeds: new[] { builder.Build() }, ephemeral: true);
    }

    private async Task Client_MessageReceived(SocketMessage message)
    {
        // If the message is not a user message, we don't need to handle it
        if (message is not SocketUserMessage userMessage) return;

        // If the message is from this bot, or any other bot, we don't need to handle it
        if (userMessage.Author.Id == _client.CurrentUser.Id || userMessage.Author.IsBot) return;
            
        // Only accept messages from users currently on the server
        if (!IsUserOnServer((DiscordUserId)message.Author.Id))
            return;

        // Check for spam
        await CheckForSpam(userMessage);
    }
}