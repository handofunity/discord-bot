using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Exceptions;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;
using CommandInfo = Discord.Commands.CommandInfo;

namespace HoU.GuildBot.DAL.Discord
{
    [UsedImplicitly]
    public class DiscordAccess : IDiscordAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private static readonly Dictionary<string, Role> _roleMapping;
        private readonly ILogger<DiscordAccess> _logger;
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISpamGuard _spamGuard;
        private readonly IIgnoreGuard _ignoreGuard;
        private readonly ICommandRegistry _commandRegistry;
        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly IStaticMessageProvider _staticMessageProvider;
        private readonly IDiscordUserEventHandler _discordUserEventHandler;
        private readonly IUserStore _userStore;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly Queue<string> _pendingMessages;

        private bool _guildAvailable;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

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
                             AppSettings appSettings,
                             IServiceProvider serviceProvider,
                             ISpamGuard spamGuard,
                             IIgnoreGuard ignoreGuard,
                             ICommandRegistry commandRegistry,
                             INonMemberRoleProvider nonMemberRoleProvider,
                             IGameRoleProvider gameRoleProvider,
                             IStaticMessageProvider staticMessageProvider,
                             IDiscordUserEventHandler discordUserEventHandler,
                             IUserStore userStore)
        {
            _logger = logger;
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
            _spamGuard = spamGuard;
            _ignoreGuard = ignoreGuard;
            _commandRegistry = commandRegistry;
            _gameRoleProvider = gameRoleProvider;
            _staticMessageProvider = staticMessageProvider;
            _discordUserEventHandler = discordUserEventHandler;
            _userStore = userStore;
            nonMemberRoleProvider.DiscordAccess = this;
            _gameRoleProvider.DiscordAccess = this;
            _staticMessageProvider.DiscordAccess = this;
            _discordUserEventHandler.DiscordAccess = this;
            _commands = new CommandService();
            _client = new DiscordSocketClient();
            _pendingMessages = new Queue<string>();
            
            _client.Log += LogClient;
            _commands.Log += LogCommands;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        /// <summary>
        /// Gets the "Hand of Unity" <see cref="SocketGuild"/> object.
        /// </summary>
        /// <returns>A <see cref="SocketGuild"/> instance for "Hand of Unity".</returns>
        private SocketGuild GetGuild() => _client.GetGuild(_appSettings.HandOfUnityGuildId);

        private bool IsUserOnServer(DiscordUserID userID)
        {
            // Cannot check if the user store is not initialized
            if (!_userStore.IsInitialized)
                return false;
            // Even if the user store is there to check,
            // checking any further is meaningless if the guild (server) is not available
            if (!_guildAvailable)
                return false;

            // If the user can be found in the store, he's on the server
            return _userStore.TryGetUser(userID, out _);
        }

        private async Task<bool> IsSpam(SocketMessage userMessage)
        {
            // If the message was received on a direct message channel, it's never spam
            if (userMessage.Channel is IPrivateChannel)
                return false;
            
            var checkResult = _spamGuard.CheckForSpam(userMessage.Author.Id, userMessage.Channel.Id, userMessage.Content, userMessage.Attachments?.Count ?? 0);
            if (checkResult == SpamCheckResult.NoSpam)
                return false;

            var g = GetGuild();
            var leaderRole = GetRoleByName(Constants.RoleNames.LeaderRoleName, g);
            var officerRole = GetRoleByName(Constants.RoleNames.OfficerRoleName, g);

            switch (checkResult)
            {
                case SpamCheckResult.SoftLimitHit:
                {
                    var embedBuilder = new EmbedBuilder()
                                       .WithColor(Color.DarkPurple)
                                       .WithTitle("Spam warning")
                                       .WithDescription("Please refrain from further spamming in this channel.");
                    var embed = embedBuilder.Build();
                    await userMessage
                          .Channel.SendMessageAsync($"{userMessage.Author.Mention} - {leaderRole.Mention} and {officerRole.Mention} have been notified.", false, embed)
                          .ConfigureAwait(false);
                    return true;
                }
                case SpamCheckResult.BetweenSoftAndHardLimit:
                {
                    return true;
                }
                case SpamCheckResult.HardLimitHit:
                {
                    var guildUser = g.GetUser(userMessage.Author.Id);
                    try
                    {
                        await guildUser.KickAsync("Excessive spam.", RequestOptions.Default).ConfigureAwait(false);
                    }
                    catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
                    {
                        await LogToDiscordInternal(
                                $"{leaderRole.Mention}, {officerRole.Mention}: Failed to kick user {guildUser.Mention}, because the bot is not permitted to kick a user with a higher rank.")
                            .ConfigureAwait(false);
                        return true;
                    }
                    catch (Exception e)
                    {
                        await LogToDiscordInternal(
                                $"{leaderRole.Mention}, {officerRole.Mention}: Failed to kick user {guildUser.Mention} due to an unexpected error: {e.Message}")
                            .ConfigureAwait(false);
                        return true;
                    }

                    await LogToDiscordInternal($"{leaderRole.Mention}, {officerRole.Mention}: Kicked user {guildUser.Mention} from the server due to excessive spam.").ConfigureAwait(false);
                    return true;
                }
            }

            return false;
        }

        private bool ShouldIgnore(SocketMessage userMessage)
        {
            return _ignoreGuard.ShouldIgnoreMessage((DiscordUserID)userMessage.Author.Id)
                && !userMessage.Content.Contains("notice me"); // Required to disable ignore duration early
        }

        private async Task ProcessMessage(SocketUserMessage userMessage)
        {
            var argPos = 0;
            if (userMessage.HasStringPrefix("hou!", ref argPos, StringComparison.OrdinalIgnoreCase) // Take action when the prefix is at the beginning
             || userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos)) // Take action when the bot is mentioned
            {
                // If the message is a command, check for ignore-entries
                if (ShouldIgnore(userMessage)) return;

                var context = new SocketCommandContext(_client, userMessage);
                try
                {
                    await _commands.ExecuteAsync(context, argPos, _serviceProvider);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error during command processing");
                }
            }
        }

        private static string GetDescriptiveErrorReason(IResult result)
        {
            if (result.Error != null && result.Error == CommandError.UnmetPrecondition)
            {
                if (result.ErrorReason?.ToLower().Contains("invalid context for command") == true)
                {
                    return "The command used was executed in the wrong context (guild channel or direct message to the bot). " +
                           "Please use the hou!help command to view the valid contexts for the command.";
                }
            }

            // Fallback: just use the reason given
            return result.ErrorReason;
        }

        private void RegisterCommands()
        {
            Shared.Objects.CommandInfo ToSharedCommandInfo(CommandInfo ci)
            {
                // Get required context
                var rt = RequestType.Undefined;
                var requiredContext = ci.Preconditions.OfType<RequireContextAttribute>().SingleOrDefault();
                if (requiredContext != null)
                {
                    if (requiredContext.Contexts.HasFlag(ContextType.Guild))
                        rt = rt | RequestType.GuildChannel;
                    if (requiredContext.Contexts.HasFlag(ContextType.DM))
                        rt = rt | RequestType.DirectMessage;
                }

                // Get response type
                var responseType = ci.Attributes.OfType<ResponseContextAttribute>().SingleOrDefault();
                var resp = responseType?.ResponseType ?? ResponseType.Undefined;

                // Get required role
                var requiredRoles = ci.Preconditions.OfType<RolePreconditionAttribute>().SingleOrDefault();
                var rr = requiredRoles?.AllowedRoles ?? Role.NoRole;

                // Get command category
                var commandCategory = ci.Attributes.OfType<CommandCategoryAttribute>().SingleOrDefault();
                var cc = commandCategory?.Category ?? CommandCategory.Undefined;
                var cco = commandCategory?.Order ?? 0;

                // Create PoCo
                return new Shared.Objects.CommandInfo(ci.Name, ci.Aliases.ToArray(), rt, resp, rr, cc, cco)
                {
                    Summary = ci.Summary,
                    Remarks = ci.Remarks
                };
            }
            _commandRegistry.RegisterAndValidateCommands(_commands.Commands.Select(ToSharedCommandInfo).ToArray());
        }

        private static Role SocketRoleToRole(IReadOnlyCollection<SocketRole> roles)
        {
            var r = Role.NoRole;
            foreach (var socketRole in roles)
            {
                if (_roleMapping.TryGetValue(socketRole.Name, out var role))
                    r = r | role;
            }

            return r;
        }

        private async Task LogToDiscordInternal(string message)
        {
            var g = GetGuild();
            var lc = g?.GetTextChannel((ulong)_appSettings.LoggingChannelId);
            if (lc == null)
            {
                // Guild or channel can be null because the guild is currently unavailable.
                // In this case, store the messages in a queue and send them later.
                // Otherwise throw.
                if (_guildAvailable)
                    throw new ChannelNotFoundException(_appSettings.LoggingChannelId);
                _pendingMessages.Enqueue(message);
                return;
            }

            await lc.SendMessageAsync(message).ConfigureAwait(false);
        }

        private SocketGuildUser GetGuildUserById(DiscordUserID userId)
        {
            var g = GetGuild();
            return g.GetUser((ulong)userId);
        }

        private IRole GetRoleByName(string name, SocketGuild guild = null)
        {
            var g = guild ?? GetGuild();
            return g.Roles.SingleOrDefault(m => m.Name == name);
        }

        private LogLevel ToLogLevel(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return LogLevel.Critical;
                case LogSeverity.Error:
                    return LogLevel.Error;
                case LogSeverity.Warning:
                    return LogLevel.Warning;
                case LogSeverity.Info:
                    return LogLevel.Information;
                case LogSeverity.Verbose:
                    return LogLevel.Trace;
                case LogSeverity.Debug:
                    return LogLevel.Debug;
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }

        private void LogInternal(string prefix, LogMessage msg)
        {
            // Locals
            var handled = false;

            static string FormatLogMessage(string m, Exception e)
            {
                if (m != null && e != null)
                    return $"{m}: {e}";
                return e?.ToString() ?? m;
            }

            // Handle special cases
            if (msg.Exception?.InnerException is WebSocketClosedException webSocketClosedException)
            {
                // In case of WebSocketClosedException, check for expected behavior, give the exception more meaning and log only the inner exception data
                if (webSocketClosedException.CloseCode == 1001)
                {
                    _logger.Log(ToLogLevel(msg.Severity), 0, prefix + $"The server sent close 1001 [Going away]: {(string.IsNullOrWhiteSpace(webSocketClosedException.Reason) ? "<no further reason specified>" : webSocketClosedException.Reason)}", null, FormatLogMessage);
                    handled = true;
                }
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
                        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
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

        private bool CanBotModifyUser(DiscordUserID userID)
        {
            var gu = GetGuildUserById(userID);
            return CanBotModifyUser(gu);
        }

        private bool CanBotModifyUser(SocketGuildUser user)
        {
            var botUser = GetGuildUserById((DiscordUserID)_client.CurrentUser.Id);
            return botUser.Roles.Max(m => m.Position) > user.Roles.Max(m => m.Position);
        }

        private async Task UpdateGuildUserRoles(DiscordUserID userID, Role oldRoles, Role newRoles)
        {
            var result = _discordUserEventHandler.HandleRolesChanged(userID, oldRoles, newRoles);
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
                    var gu = GetGuildUserById(userID);
                    var username = gu.Username;
                    var validCharacters = new Microsoft.AspNetCore.Identity.UserOptions().AllowedUserNameCharacters;
                    var valid = username.All(character => validCharacters.Contains(character));
                    if (valid)
                        return true;

                    // Get channels for notifications
                    var privateChannel = await gu.GetOrCreateDMChannelAsync();
                    var comCoordinatorChannel = g.GetTextChannel((ulong) _appSettings.ComCoordinatorChannelId);

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

                    var trialMemberRole = GetRoleByName(nameof(Role.TrialMember), g);
                    await gu.RemoveRoleAsync(trialMemberRole);

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to verify username characters of user {DiscordUserId}.", userID);
                    return true;
                }
            }

            async Task AnnouncePromotion()
            {
                try
                {
                    // Log promotion
                    await LogToDiscordInternal(result.LogMessage).ConfigureAwait(false);

                    // Announce promotion
                    var g = GetGuild().GetTextChannel((ulong) _appSettings.PromotionAnnouncementChannelId);
                    var embed = result.AnnouncementData.ToEmbed();
                    await g.SendMessageAsync(string.Empty, false, embed).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Promotion announcement for user {DiscordUserId} failed.", userID);
                }
            }
        }

        private async Task VerifyRoles(DiscordUserID discordUserID,
                                       bool isGuildMember,
                                       IEnumerable<SocketRole> previousRoles,
                                       IEnumerable<SocketRole> currentRoles)
        {
            if (isGuildMember)
                return;

            var newRoles = currentRoles.Except(previousRoles).ToArray();
            if (newRoles.Length == 0)
                return;

            var invalidRoles = _gameRoleProvider.Games.Where(m => m.AvailableRoles != null
                                                                  && m.AvailableRoles.Count > 0)
                                                .Join(newRoles,
                                                      game => $"{game.LongName} ({game.ShortName})".ToLowerInvariant(),
                                                      role => role.Name.ToLowerInvariant(),
                                                      (game,
                                                       role) => role.Name)
                                                .ToArray();
            if (invalidRoles.Length == 0)
                return;

            var g = GetGuild();
            var gu = GetGuildUserById(discordUserID);
            var leaderRole = GetRoleByName(Constants.RoleNames.LeaderRoleName, g);
            var officerRole = GetRoleByName(Constants.RoleNames.OfficerRoleName, g);
            foreach (var invalidRole in invalidRoles)
            {
                await LogToDiscordInternal($"{leaderRole.Mention} {officerRole.Mention}: User `{gu.Username}#{gu.DiscriminatorValue}` " +
                                           $"is no guild member but was assigned the role `{invalidRole}`. " +
                                           "Please verify the correctness of this role assignment.").ConfigureAwait(false);
            }
        }

        private async Task ApplyGroupRoles(DiscordUserID discordUserId,
                                           IReadOnlyCollection<SocketRole> previousRoles,
                                           IReadOnlyCollection<SocketRole> currentRoles)
        {
            if (!CanBotModifyUser(discordUserId))
                return;

            const string groupRolePrefix = "●";
            var rolesRemoved = previousRoles.Except(currentRoles)
                                        .Any(m => !m.Name.StartsWith(groupRolePrefix));
            var rolesAdded = currentRoles.Except(previousRoles)
                                       .Any(m => !m.Name.StartsWith(groupRolePrefix));
            if (!rolesRemoved && !rolesAdded)
                return;

            var g = GetGuild();
            var groupRoles = g.Roles
                              .Where(m => m.Name.StartsWith(groupRolePrefix))
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

            try
            {
                var gu = GetGuildUserById(discordUserId);
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
                _logger.LogError(e, "Failed to add or remove user {Username} ({DiscordUserId}) to or from group roles.");
            }

            static string TrimRoleName(string roleName)
            {
                var result = roleName.Trim();
                while (result.Contains("\u2063"))
                    result = result.Replace("\u2063", string.Empty);
                while (result.Contains("\u2002"))
                    result = result.Replace("\u2002", string.Empty);

                return result;
            }
        }

        private static bool IsOnline(IPresence gu) => gu.Status != UserStatus.Offline && gu.Status != UserStatus.Invisible;

        private SocketGuildUser[] GetGuildMembersWithRoles(ulong[] roleIDs, ulong[] roleIDsToExclude)
        {
            var g = GetGuild();
            return g.Users
                    .Where(m => _userStore.TryGetUser((DiscordUserID) m.Id, out var user)
                                && user.IsGuildMember
                                && (roleIDs == null || roleIDs.Intersect(m.Roles.Select(r => r.Id)).Count() == roleIDs.Length)
                                && (roleIDsToExclude == null || !roleIDsToExclude.Intersect(m.Roles.Select(r => r.Id)).Any()))
                    .ToArray();
        }

        private SocketGuildUser[] GetGuildMembersWithAnyGivenRole(SocketGuild guild,
                                                                  ulong[] roleIDs)
        {
            return guild.Users
                        .Where(m => _userStore.TryGetUser((DiscordUserID) m.Id, out var user)
                                    && user.IsGuildMember
                                    && (roleIDs == null || roleIDs.Intersect(m.Roles.Select(r => r.Id)).Any()))
                        .ToArray();
        }

        private static EmojiDefinition TryParseToEmojiDefinition(IEmote e)
        {
            switch (e)
            {
                case Emoji emoji:
                    return EmojiDefinition.AllEmojis.SingleOrDefault(m => m.Unicode == emoji.Name);
                case Emote emote:
                    var matchByName = EmojiDefinition.AllEmojis
                                                     .Where(m => m.Name == emote.Name)
                                                     .ToArray();
                    return matchByName.SingleOrDefault(m => m.Id.HasValue && m.Id.Value == emote.Id
                                                            || m.EmojiKind == EmojiDefinition.Kind.ReadonlyCustomEmote);
                default:
                    throw new NotSupportedException("Unknown emote type.");
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDiscordAccess Members

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
                    // Fire & Forget
                    Task.Run(async () =>
                    {
                        if (!_commandRegistry.CommandsRegistered)
                        {
                            // Load modules and register commands only once
                            await _commands.AddModulesAsync(typeof(DiscordAccess).Assembly, _serviceProvider).ConfigureAwait(false);
                            RegisterCommands();
                        }
                        await connectedHandler().ConfigureAwait(false);
                        _commands.CommandExecuted += Commands_CommandExecuted;
                        _client.MessageReceived += Client_MessageReceived;
                    });
                    // Return immediately
                    return Task.CompletedTask;
                };
            }

            Func<Exception, Task> ClientDisconnected()
            {
                return exception =>
                {
                    // Fire & Forget
                    Task.Run(async () =>
                    {
                        _client.MessageReceived -= Client_MessageReceived;
                        _commands.CommandExecuted -= Commands_CommandExecuted;
                        await disconnectedHandler().ConfigureAwait(false);
                    });
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
                _client.GuildMemberUpdated -= Client_GuildMemberUpdated;
                _client.GuildMemberUpdated += Client_GuildMemberUpdated;
                _client.ReactionRemoved -= Client_ReactionRemoved;
                _client.ReactionRemoved += Client_ReactionRemoved;
                _client.ReactionAdded -= Client_ReactionAdded;
                _client.ReactionAdded += Client_ReactionAdded;

                _logger.LogInformation("Performing login...");
                await _client.LoginAsync(TokenType.Bot, _appSettings.BotToken).ConfigureAwait(false);
                _logger.LogInformation("Starting client...");
                await _client.StartAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while connecting to Discord.");
            }
        }

        async Task IDiscordAccess.SetCurrentGame(string gameName)
        {
            await _client.SetGameAsync(gameName).ConfigureAwait(false);
        }

        async Task IDiscordAccess.LogToDiscord(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(nameof(message));

            await LogToDiscordInternal(message).ConfigureAwait(false);
        }

        bool IDiscordAccess.IsUserOnline(DiscordUserID userID)
        {
            var gu = GetGuildUserById(userID);
            return IsOnline(gu);
        }

        Dictionary<DiscordUserID, string> IDiscordAccess.GetUserNames(IEnumerable<DiscordUserID> userIDs) => userIDs.Select(GetGuildUserById).ToDictionary(gu => (DiscordUserID)gu.Id, gu => gu.Username);

        async Task<bool> IDiscordAccess.TryAddNonMemberRole(DiscordUserID userID,
                                                            Role targetRole)
        {
            var roleDisplayName = _roleMapping.Single(m => m.Value == targetRole).Key;
            var role = GetRoleByName(roleDisplayName);
            var gu = GetGuildUserById(userID);
            if (gu.Roles.Any(m => m.Id == role.Id))
                return false;
            try
            {
                await gu.AddRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to set game role '{role.Name}' for UserID {userID}.");
                return false;
            }
        }

        async Task<(bool Success, string RoleName)> IDiscordAccess.TryAddNonMemberRole(DiscordUserID userID,
                                                                                       ulong targetRole)
        {
            var g = GetGuild();
            var role = g.GetRole(targetRole);
            var gu = GetGuildUserById(userID);
            if (gu.Roles.Any(m => m.Id == role.Id))
                return (false, role.Name);
            try
            {
                await gu.AddRoleAsync(role).ConfigureAwait(false);
                return (true, role.Name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to set game role '{role.Name}' for UserID {userID}.");
                return (false, role.Name);
            }
        }

        async Task<bool> IDiscordAccess.TryRevokeNonMemberRole(DiscordUserID userID,
                                                               Role targetRole)
        {
            var roleDisplayName = _roleMapping.Single(m => m.Value == targetRole).Key;
            var role = GetRoleByName(roleDisplayName);
            var gu = GetGuildUserById(userID);
            if (gu.Roles.All(m => m.Id != role.Id))
                return false;
            try
            {
                await gu.RemoveRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to revoke game role '{role.Name}' for UserID {userID}.");
                return false;
            }
        }

        async Task<(bool Success, string RoleName)> IDiscordAccess.TryRevokeNonMemberRole(DiscordUserID userID,
                                                                                          ulong targetRole)
        {
            var g = GetGuild();
            var role = g.GetRole(targetRole);
            var gu = GetGuildUserById(userID);
            if (gu.Roles.All(m => m.Id != role.Id))
                return (false, role.Name);
            try
            {
                await gu.RemoveRoleAsync(role).ConfigureAwait(false);
                return (true, role.Name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to revoke game role '{role.Name}' for UserID {userID}.");
                return (false, role.Name);
            }
        }

        async Task<bool> IDiscordAccess.TryAddPrimaryGameRole(DiscordUserID userID,
                                                              AvailableGame game)
        {
            var gu = GetGuildUserById(userID);
            var role = gu.Guild.Roles.Single(m => m.Id == game.PrimaryGameDiscordRoleID);
            if (gu.Roles.Any(m => m.Id == game.PrimaryGameDiscordRoleID))
                return false;
            try
            {
                await gu.AddRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to set primary game role for game '{game.LongName}' for UserID {userID}.");
                return false;
            }
        }

        async Task<bool> IDiscordAccess.TryRevokePrimaryGameRole(DiscordUserID userID,
                                                                 AvailableGame game)
        {
            var gu = GetGuildUserById(userID);
            var role = gu.Guild.Roles.Single(m => m.Id == game.PrimaryGameDiscordRoleID);
            if (gu.Roles.All(m => m.Id != game.PrimaryGameDiscordRoleID))
                return false;
            try
            {
                await gu.RemoveRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to remove primary game role for game '{game.LongName}' for UserID {userID}.");
                return false;
            }
        }

        async Task<bool> IDiscordAccess.TryAddGameRole(DiscordUserID userID, AvailableGame game, string className)
        {
            var role = GetRoleByName($"{game.ShortName} - {className}");
            var gu = GetGuildUserById(userID);
            if (gu.Roles.Any(m => m.Id == role.Id))
                return false;
            try
            {
                await gu.AddRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to set game role '{role.Name}' for UserID {userID}.");
                return false;
            }
        }

        async Task<bool> IDiscordAccess.TryRevokeGameRole(DiscordUserID userID, AvailableGame game, string className)
        {
            var role = GetRoleByName($"{game.ShortName} - {className}");
            var gu = GetGuildUserById(userID);
            if (gu.Roles.All(m => m.Id != role.Id))
                return false;
            try
            {
                await gu.RemoveRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to revoke game role '{role.Name}' for UserID {userID}.");
                return false;
            }
        }

        bool IDiscordAccess.CanManageRolesForUser(DiscordUserID userID)
        {
            return CanBotModifyUser(userID);
        }

        string IDiscordAccess.GetRoleMention(string roleName)
        {
            return GetRoleByName(roleName).Mention;
        }

        async Task<TextMessage[]> IDiscordAccess.GetBotMessagesInChannel(DiscordChannelID channelID)
        {
            var result = new List<TextMessage>();
            var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelID);
            var messageCollection = channel.GetMessagesAsync();
            var enumerator = messageCollection.GetAsyncEnumerator();
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                if (enumerator.Current != null)
                {
                    foreach (var message in enumerator.Current.Where(m => m.Author.Id == _client.CurrentUser.Id))
                    {
                        result.Add(new TextMessage((DiscordChannelID)message.Channel.Id, message.Id, message.Content));
                    }
                }
            }

            result.Reverse();
            return result.ToArray();
        }

        async Task IDiscordAccess.DeleteBotMessagesInChannel(DiscordChannelID channelID)
        {
            var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelID);
            var messagesToDelete = new List<IMessage>();
            var messageCollection = channel.GetMessagesAsync();
            var enumerator = messageCollection.GetAsyncEnumerator();
            _logger.LogTrace($"Fetching messages to delete in channel {channelID} ...");
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
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
                    _logger.LogTrace($"Channel {channelID}: Deleting message {current}/{messagesToDelete.Count} with ID {message.Id}.");
                    await message.DeleteAsync().ConfigureAwait(false);
                    _logger.LogTrace($"Channel {channelID}: Deleted message {current}/{messagesToDelete.Count} with ID {message.Id}.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to delete message with ID {message.Id} in channel {channelID}.");
                }
            }).ConfigureAwait(false);
        }

        async Task IDiscordAccess.DeleteBotMessageInChannel(DiscordChannelID channelID, ulong messageID)
        {
            var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelID);
            var message = await channel.GetMessageAsync(messageID).ConfigureAwait(false);
            await message.DeleteAsync().ConfigureAwait(false);
        }

        async Task<ulong[]> IDiscordAccess.CreateBotMessagesInChannel(DiscordChannelID channelID, string[] messages)
        {
            var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelID);
            var result = new List<ulong>();

            var current = 0;
            await messages.PerformBulkOperation(async message =>
            {
                current++;
                _logger.LogTrace($"Channel {channelID}: Creating message {current}/{messages.Length} ...");
                var createdMessage = await channel.SendMessageAsync(message).ConfigureAwait(false);
                result.Add(createdMessage.Id);
                _logger.LogTrace($"Channel {channelID}: Created message {current}/{messages.Length} with ID {createdMessage.Id}.");
            }).ConfigureAwait(false);

            return result.ToArray();
        }

        async Task IDiscordAccess.CreateBotMessageInWelcomeChannel(string message)
        {
            var g = GetGuild();
            await g.SystemChannel.SendMessageAsync(message).ConfigureAwait(false);
        }

        async Task IDiscordAccess.AddReactionsToMessage(DiscordChannelID channelID, ulong messageID, EmojiDefinition[] reactions)
        {
            var channel = (ITextChannel)GetGuild().GetChannel((ulong)channelID);
            var message = (IUserMessage)await channel.GetMessageAsync(messageID).ConfigureAwait(false);

            await reactions.PerformBulkOperation(async reaction =>
            {
                IEmote emote;
                if (reaction.EmojiKind == EmojiDefinition.Kind.UnicodeEmoji)
                    emote = new Emoji(reaction.Unicode);
                else
                    emote = Emote.Parse($"<:{reaction.Name}:{reaction.Id.Value}>");

                await message.AddReactionAsync(emote).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        int IDiscordAccess.CountGuildMembersWithRoles(ulong[] roleIDs)
        {
            var guildMembers = GetGuildMembersWithRoles(roleIDs, null);
            return guildMembers.Length;
        }

        int IDiscordAccess.CountGuildMembersWithRoles(ulong[] roleIDs,
                                                      ulong[] roleIDsToExclude)
        {
            var guildMembers = GetGuildMembersWithRoles(roleIDs, roleIDsToExclude);
            return guildMembers.Length;
        }

        int IDiscordAccess.CountGuildMembersWithRoles(string[] roleNames)
        {
            var roleIDs = new List<ulong>();
            foreach (var roleName in roleNames)
            {
                roleIDs.Add(GetRoleByName(roleName).Id);
            }
            var guildMembers = GetGuildMembersWithRoles(roleIDs.ToArray(), null);
            return guildMembers.Length;
        }

        bool IDiscordAccess.DoesRoleExist(ulong roleID)
        {
            var g = GetGuild();
            return g.Roles.Any(m => m.Id == roleID);
        }

        string IDiscordAccess.GetCurrentDisplayName(DiscordUserID userID)
        {
            var gu = GetGuildUserById(userID);
            return string.IsNullOrWhiteSpace(gu.Nickname) ? gu.Username : gu.Nickname;
        }

        string IDiscordAccess.GetChannelLocationAndName(DiscordChannelID discordChannelID)
        {
            var g = GetGuild();
            var channel = g.TextChannels.Single(m => m.Id == (ulong) discordChannelID);
            return $"/{channel.Category.Name}/{channel.Name}";
        }

        async Task<(ulong VoiceChannelId, string Error)> IDiscordAccess.CreateVoiceChannel(ulong voiceChannelsCategoryId,
                                                                                           string name,
                                                                                           int maxUsers)
        {
            var g = GetGuild();
            try
            {
                if (g.VoiceChannels.Any(m => m.Name == name))
                    return (0, "Voice channel with same name already exists.");

                var voiceChannel = await g.CreateVoiceChannelAsync(name,
                                                                   properties =>
                                                                   {
                                                                       properties.UserLimit = maxUsers;
                                                                       properties.CategoryId = voiceChannelsCategoryId;
                                                                   });
                return (voiceChannel.Id, null);
            }
            catch (Exception e)
            {
                return (0, e.Message);
            }
        }

        async Task IDiscordAccess.ReorderChannelsAsync(ulong[] channelIds,
                                                       ulong positionAboveChannelId)
        {
            var g = GetGuild();
            try
            {
                var baseChannel = g.GetChannel(positionAboveChannelId) as INestedChannel;
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
                                                            .Where(m => !channelIds.Contains(m.Id))
                                                            .OrderBy(m => m.Position)
                                                            .ToArray();

                var position = 1;
                foreach (var channel in channelsInCategory)
                {
                    if (channel.Id == positionAboveChannelId)
                    {
                        foreach (var channelId in channelIds)
                        {
                            finalList.Add(new ReorderChannelProperties(channelId, position));
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

        async Task<bool> IDiscordAccess.DeleteVoiceChannelIfEmpty(ulong voiceChannelId)
        {
            var g = GetGuild();
            var voiceChannel = g.GetVoiceChannel(voiceChannelId);
            if (voiceChannel == null)
                return true;

            if (voiceChannel.Users.Count > 0)
                return false;

            await voiceChannel.DeleteAsync();
            return true;
        }

        async Task IDiscordAccess.DeleteVoiceChannel(ulong voiceChannelId)
        {
            var g = GetGuild();
            var voiceChannel = g.GetVoiceChannel(voiceChannelId);
            if (voiceChannel == null)
                return;

            await voiceChannel.DeleteAsync();
        }

        DiscordChannelID? IDiscordAccess.GetUsersVoiceChannelId(DiscordUserID userId)
        {
            var gu = GetGuildUserById(userId);
            return (DiscordChannelID?)gu.VoiceChannel?.Id;
        }

        async Task<bool> IDiscordAccess.SetUsersMuteStateInVoiceChannel(DiscordChannelID voiceChannelId,
                                                                        bool mute)
        {
            var g = GetGuild();
            var voiceChannel = g.GetVoiceChannel((ulong) voiceChannelId);
            var botGuildUser = GetGuildUserById((DiscordUserID) _client.CurrentUser.Id);
            var permissions = botGuildUser.GetPermissions(voiceChannel);
            if (!permissions.MuteMembers)
                return false;

            foreach (var voiceChannelUser in voiceChannel.Users)
            {
                if (CanBotModifyUser(voiceChannelUser)
                    && voiceChannelUser.IsMuted != mute)
                    await voiceChannelUser.ModifyAsync(properties => properties.Mute = mute)
                                          .ConfigureAwait(false);
            }

            return true;
        }

        string IDiscordAccess.GetAvatarId(DiscordUserID userId)
        {
            var gu = GetGuildUserById(userId);
            return gu.AvatarId;
        }

        UserModel[] IDiscordAccess.GetUsersInRoles(string[] allowedRoles)
        {
            var g = GetGuild();
            if (g == null)
                return new UserModel[0];

            var allowedRoleIds = allowedRoles.Select(allowedRole => GetRoleByName(allowedRole, g))
                                             .Where(m => m != null)
                                             .Select(m => m.Id)
                                             .ToArray();
            var users = GetGuildMembersWithAnyGivenRole(g, allowedRoleIds);
            return users.Select(m => new UserModel
                         {
                             DiscordUserId = (DiscordUserID)m.Id,
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
            var g = GetGuild();
            var leaderRole = GetRoleByName(Constants.RoleNames.LeaderRoleName, g);
            var officerRole = GetRoleByName(Constants.RoleNames.OfficerRoleName, g);
            return $"{leaderRole.Mention} {officerRole.Mention}";
        }

        async Task IDiscordAccess.SendUnitsNotificationAsync(EmbedData embedData)
        {
            try
            {
                var g = GetGuild();
                var channel = g.GetTextChannel((ulong)_appSettings.UnitsNotificationsChannelId);
                var embed = embedData.ToEmbed();
                await channel.SendMessageAsync(null, false, embed).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send UNITS notification.");
            }
        }

        public async Task SendUnitsNotificationAsync(EmbedData embedData,
                                                     DiscordUserID[] usersToNotify)
        {
            try
            {
                var g = GetGuild();
                var channel = g.GetTextChannel((ulong)_appSettings.UnitsNotificationsChannelId);
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
                    string addToNext = null;
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
                await channel.SendMessageAsync(notifications[0], false, embed).ConfigureAwait(false);
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

        Dictionary<string, List<DiscordUserID>> IDiscordAccess.GetUsersInVoiceChannels(string[] voiceChannelIds)
        {
            var g = GetGuild();
            var result = new Dictionary<string, List<DiscordUserID>>();
            foreach (var voiceChannelIdStr in voiceChannelIds)
            {
                if (!ulong.TryParse(voiceChannelIdStr, out var voiceChannelId))
                    continue;

                var voiceChannel = g.GetVoiceChannel(voiceChannelId);
                var userIds = voiceChannel.Users
                                          .Select(m => (DiscordUserID) m.Id)
                                          .ToList();
                if (userIds.Any())
                    result.Add(voiceChannelIdStr, userIds);
            }

            return result;
        }

        List<DiscordUserID> IDiscordAccess.GetUsersIdsInRole(ulong roleId)
        {
            var g = GetGuild();
            var role = g.GetRole(roleId);
            return role.Members.Select(roleMember => (DiscordUserID) roleMember.Id).ToList();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Event Handler

        private Task LogClient(LogMessage arg)
        {
            LogInternal($"{nameof(DiscordSocketClient)}: ", arg);
            return Task.CompletedTask;
        }

        private Task LogCommands(LogMessage arg)
        {
            LogInternal($"{nameof(CommandService)}: ", arg);
            return Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(SocketGuild guild)
        {
            if (guild.Id != _appSettings.HandOfUnityGuildId)
                return;

            _guildAvailable = true;
            _logger.LogInformation($"Guild '{guild.Name}' is available.");
            _ = Task.Run(async () =>
            {
                if (!_userStore.IsInitialized)
                {
                    // Initialize user store only once
                    await guild.DownloadUsersAsync();
                    var allGuildUsers = guild.Users;
                    var mappedGuildUsers = allGuildUsers.Select(m => ((DiscordUserID) m.Id, SocketRoleToRole(m.Roles))).ToArray();
                    await _userStore.Initialize(mappedGuildUsers);
                }
                
                if (_gameRoleProvider.Games.Count == 0)
                {
                    // Load games only once
                    await _gameRoleProvider.LoadAvailableGames().ConfigureAwait(false);
                }

                // Ensure that static messages exist
                await _staticMessageProvider.EnsureStaticMessagesExist().ConfigureAwait(false);

            }).ConfigureAwait(false);

            while (_pendingMessages.Count > 0)
            {
                var pm = _pendingMessages.Dequeue();
                await LogToDiscordInternal(pm).ConfigureAwait(false);
            }
        }

        private Task Client_GuildUnavailable(SocketGuild guild)
        {
            if (guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;

            _guildAvailable = false;

            return Task.CompletedTask;
        }

        private Task Client_UserJoined(SocketGuildUser guildUser)
        {
            _discordUserEventHandler.HandleJoined((DiscordUserID)guildUser.Id, SocketRoleToRole(guildUser.Roles));
            return Task.CompletedTask;
        }

        private Task Client_UserLeft(SocketGuildUser guildUser)
        {
            _discordUserEventHandler.HandleLeft((DiscordUserID) guildUser.Id,
                                                guildUser.Username,
                                                guildUser.DiscriminatorValue,
                                                guildUser.JoinedAt,
                                                guildUser.Roles.Where(m => !m.IsEveryone).Select(m => m.Name).ToArray());
            return Task.CompletedTask;
        }

        private Task Client_GuildMemberUpdated(SocketGuildUser oldGuildUser, SocketGuildUser newGuildUser)
        {
            if (oldGuildUser.Guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;
            if (newGuildUser.Guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;
            if (oldGuildUser.Id != newGuildUser.Id)
                return Task.CompletedTask;

            // Fire & Forget
            Task.Run(async () =>
            {
                var discordUserId = (DiscordUserID) newGuildUser.Id;
                // Handle possible role change
                var oldRoles = SocketRoleToRole(oldGuildUser.Roles);
                var newRoles = SocketRoleToRole(newGuildUser.Roles);
                if (oldRoles != newRoles)
                {
                    await UpdateGuildUserRoles(discordUserId, oldRoles, newRoles).ConfigureAwait(false);
                }

                // Handle possible role change, that only should be there for guild members
                if (_userStore.TryGetUser(discordUserId, out var user))
                {
                    await VerifyRoles(user.DiscordUserID,
                                      user.IsGuildMember,
                                      oldGuildUser.Roles,
                                      newGuildUser.Roles)
                       .ConfigureAwait(false);
                }

                // Handle possible role change, that would end up in new or removed group roles
                await ApplyGroupRoles(discordUserId,
                                      oldGuildUser.Roles,
                                      newGuildUser.Roles)
                   .ConfigureAwait(false);

                // Handle possible status change
                if (oldGuildUser.Status != newGuildUser.Status)
                {
                    var wasOnline = IsOnline(oldGuildUser);
                    var isOnline = IsOnline(newGuildUser);
                    await _discordUserEventHandler.HandleStatusChanged(discordUserId, wasOnline, isOnline).ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        private Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // Don't handle reactions where the user is not specified
            // Don't handle bot reactions
            if (!reaction.User.IsSpecified
              || reaction.User.Value.IsBot)
            {
                return Task.CompletedTask;
            }

            var parsedEmoji = TryParseToEmojiDefinition(reaction.Emote);
            // Fire & Forget
            Task.Run(async () => await _discordUserEventHandler
                                       .HandleReactionRemoved((DiscordChannelID)channel.Id, (DiscordUserID)reaction.UserId, message.Id, parsedEmoji, reaction.Emote.Name)
                                       .ConfigureAwait(false)).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // Don't handle reactions where the user is not specified
            // Don't handle bot reactions
            if (!reaction.User.IsSpecified
                || reaction.User.Value.IsBot)
            {
                return Task.CompletedTask;
            }

            var parsedEmoji = TryParseToEmojiDefinition(reaction.Emote);
            // Fire & Forget
            Task.Run(async () => await _discordUserEventHandler
                                       .HandleReactionAdded((DiscordChannelID) channel.Id, (DiscordUserID) reaction.UserId, message.Id, parsedEmoji, reaction.Emote.Name)
                                       .ConfigureAwait(false)).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        private async Task Client_MessageReceived(SocketMessage message)
        {
            // If the message is not a user message, we don't need to handle it
            SocketUserMessage userMessage;
            if ((userMessage = message as SocketUserMessage) == null) return;

            // If the message is from this bot, or any other bot, we don't need to handle it
            if (userMessage.Author.Id == _client.CurrentUser.Id || userMessage.Author.IsBot) return;
            
            // Only accept messages from users currently on the server
            if (!IsUserOnServer((DiscordUserID)message.Author.Id))
                return;

            // Check for spam
            if (await IsSpam(userMessage).ConfigureAwait(false)) return;
            
            // If the message is no spam, process message
            await ProcessMessage(userMessage).ConfigureAwait(false);
        }

        private async Task Commands_CommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            var userMessage = context.Message;
            if (result != null
             && !result.IsSuccess
             && result.Error != CommandError.UnknownCommand)
            {
                // Handle error during command execution
                var isGuildChannel = userMessage.Channel is IGuildChannel;
                var embedBuilder = new EmbedBuilder()
                                  .WithColor(Color.Red)
                                  .WithTitle("Error during command execution")
                                  .WithDescription("The command you used caused an error. " +
                                                   (isGuildChannel ? "The original message was deleted to protect sensitive data and to prevent spam. " : string.Empty) +
                                                   "Please review the error reason below, copy the original message, fix any errors and try again. " +
                                                   "If you need further assistance, use the @Developer mention in any guild channel.")
                                  .AddField("Original message", userMessage.Content)
                                  .AddField("Error reason", GetDescriptiveErrorReason(result));

                if (isGuildChannel)
                    await userMessage.DeleteAsync().ConfigureAwait(false);

                var embed = embedBuilder.Build();
                _logger.LogWarning(result.ErrorReason);
                var directChannel = await userMessage.Author.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await directChannel.SendMessageAsync(string.Empty, false, embed).ConfigureAwait(false);
            }
        }

        #endregion
    }
}