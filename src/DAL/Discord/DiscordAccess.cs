﻿namespace HoU.GuildBot.DAL.Discord
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using global::Discord;
    using global::Discord.Commands;
    using global::Discord.Net;
    using global::Discord.WebSocket;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Exceptions;
    using Shared.Objects;
    using Shared.StrongTypes;
    using CommandInfo = global::Discord.Commands.CommandInfo;

    [UsedImplicitly]
    public class DiscordAccess : IDiscordAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private static readonly Dictionary<string, Role> RoleMapping;
        private readonly ILogger<DiscordAccess> _logger;
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISpamGuard _spamGuard;
        private readonly IIgnoreGuard _ignoreGuard;
        private readonly ICommandRegistry _commandRegistry;
        private readonly IMessageProvider _messageProvider;
        private readonly IGameRoleProvider _gameRoleProvider;
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
            RoleMapping = new Dictionary<string, Role>
            {
                {"Developer", Role.Developer},
                {"Leader", Role.Leader},
                {"Senior Officer", Role.SeniorOfficer},
                {"Officer", Role.Officer},
                {"Member", Role.Member},
                {"Recruit", Role.Recruit},
                {"Guest", Role.Guest}
            };
        }

        public DiscordAccess(ILogger<DiscordAccess> logger,
                             AppSettings appSettings,
                             IServiceProvider serviceProvider,
                             ISpamGuard spamGuard,
                             IIgnoreGuard ignoreGuard,
                             ICommandRegistry commandRegistry,
                             IMessageProvider messageProvider,
                             IGameRoleProvider gameRoleProvider,
                             IDiscordUserEventHandler discordUserEventHandler,
                             IUserStore userStore)
        {
            _logger = logger;
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
            _spamGuard = spamGuard;
            _ignoreGuard = ignoreGuard;
            _commandRegistry = commandRegistry;
            _messageProvider = messageProvider;
            _gameRoleProvider = gameRoleProvider;
            _discordUserEventHandler = discordUserEventHandler;
            _userStore = userStore;
            _gameRoleProvider.DiscordAccess = this;
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
            var seniorOfficerRole = GetRoleByName(Constants.RoleNames.SeniorOfficerRoleName, g);

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
                          .Channel.SendMessageAsync($"{userMessage.Author.Mention} - {leaderRole.Mention} and {seniorOfficerRole.Mention} have been notified.", false, embed)
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
                        await guildUser.KickAsync("Excesive spam.", RequestOptions.Default).ConfigureAwait(false);
                    }
                    catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
                    {
                        await LogToDiscordInternal(
                                $"{leaderRole.Mention}, {seniorOfficerRole.Mention}: Failed to kick user {guildUser.Mention}, because the bot is not permitted to kick a user with a higher rank.")
                            .ConfigureAwait(false);
                        return true;
                    }
                    catch (Exception e)
                    {
                        await LogToDiscordInternal(
                                $"{leaderRole.Mention}, {seniorOfficerRole.Mention}: Failed to kick user {guildUser.Mention} due to an unexpected error: {e.Message}")
                            .ConfigureAwait(false);
                        return true;
                    }

                    await LogToDiscordInternal($"{leaderRole.Mention}, {seniorOfficerRole.Mention}:Kicked user {guildUser.Mention} from the server due to excesive spam.").ConfigureAwait(false);
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
                    var result = await _commands.ExecuteAsync(context, argPos, _serviceProvider).ConfigureAwait(false);
                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        // Handle error during command execution
                        var embedBuilder = new EmbedBuilder()
                                           .WithColor(Color.Red)
                                           .WithTitle("Error during command execution")
                                           .WithDescription(result.ErrorReason);
                        var embed = embedBuilder.Build();
                        _logger.LogWarning(result.ErrorReason);
                        await userMessage.Channel.SendMessageAsync(string.Empty, false, embed).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error during command processing");
                }
            }
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

                // Create POCO
                return new Shared.Objects.CommandInfo(ci.Name, ci.Aliases.ToArray(), rt, resp, rr)
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
                if (RoleMapping.TryGetValue(socketRole.Name, out var role))
                    r = r | role;
            }

            return r;
        }

        private async Task LogToDiscordInternal(string message)
        {
            var g = GetGuild();
            var lc = g.GetTextChannel(_appSettings.LoggingChannelId);
            if (lc == null)
            {
                // Channel can be null because the guild is currently unavailable.
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
            return g.Roles.Single(m => m.Name == name);
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
            string FormatLogMessage(string m, Exception e)
            {
                if (m != null && e != null)
                    return $"{m}: {e}";
                return e?.ToString() ?? m;
            }

            // Handle special cases
            if (msg.Exception?.InnerException is WebSocketClosedException wsce)
            {
                // In case of WebSocketClosedException, check for expected behavior, give the exception more meaning and log only the inner exception data
                if (wsce.CloseCode == 1001)
                {
                    _logger.Log(ToLogLevel(msg.Severity), 0, prefix + $"The server sent close 1001 [Going away]: {(string.IsNullOrWhiteSpace(wsce.Reason) ? "<no further reason specified>" : wsce.Reason)}", null, FormatLogMessage);
                    handled = true;
                }
            }

            // If the log message has been handled, don't call the default log
            if (handled) return;

            // Default log
            _logger.Log(ToLogLevel(msg.Severity), 0, prefix + msg.Message, msg.Exception, FormatLogMessage);
        }

        private async Task UpdateGuildUserRoles(DiscordUserID userID, Role oldRoles, Role newRoles)
        {
            var result = _discordUserEventHandler.HandleRolesChanged(userID, oldRoles, newRoles);
            if (result.IsPromotion)
            {
                // Log promotion
                await LogToDiscordInternal(result.LogMessage).ConfigureAwait(false);

                // Announce promotion
                var g = GetGuild().GetTextChannel(_appSettings.PromotionAnnouncementChannelId);
                var embed = result.AnnouncementData.ToEmbed();
                await g.SendMessageAsync(string.Empty, false, embed).ConfigureAwait(false);
            }
        }

        private static bool IsOnline(IPresence gu) => gu.Status != UserStatus.Offline && gu.Status != UserStatus.Invisible;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDiscordAccess Members

        bool IDiscordAccess.IsConnected => _client.ConnectionState == ConnectionState.Connected;

        async Task IDiscordAccess.Connect(Func<Task> connectedHandler, Func<Task> disconnectedHandler)
        {
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
                            await _commands.AddModulesAsync(typeof(DiscordAccess).Assembly).ConfigureAwait(false);
                            RegisterCommands();
                        }
                        await connectedHandler().ConfigureAwait(false);
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
                        await disconnectedHandler().ConfigureAwait(false);
                    });
                    // Return immediately
                    return Task.CompletedTask;
                };
            }

            if (connectedHandler == null)
                throw new ArgumentNullException(nameof(connectedHandler));

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

                await _client.LoginAsync(TokenType.Bot, _appSettings.BotToken).ConfigureAwait(false);
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

        void IDiscordAccess.GetClassNamesForGame(AvailableGame game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            var g = GetGuild();
            var roles = g.Roles.Where(m => m.Name.Contains($" ({game.ShortName})")).Select(m => m.Name.Split(' ')[0]).ToArray();
            game.ClassNames.Clear();
            game.ClassNames.AddRange(roles);
        }

        async Task IDiscordAccess.RevokeCurrentGameRoles(DiscordUserID userID, AvailableGame game)
        {
            var gu = GetGuildUserById(userID);
            var gameRelatedRoles = gu.Roles.Where(m => m.Name.Contains($" ({game.ShortName})"));
            await gu.RemoveRolesAsync(gameRelatedRoles).ConfigureAwait(false);
        }

        async Task IDiscordAccess.SetCurrentGameRole(DiscordUserID userID, AvailableGame game, string className)
        {
            var role = GetRoleByName($"{className} ({game.ShortName})");
            var gu = GetGuildUserById(userID);
            await gu.AddRoleAsync(role).ConfigureAwait(false);
        }

        bool IDiscordAccess.CanManageRolesForUser(DiscordUserID userID)
        {
            var gu = GetGuildUserById(userID);
            var botUser = GetGuildUserById((DiscordUserID)_client.CurrentUser.Id);
            return botUser.Roles.Max(m => m.Position) > gu.Roles.Max(m => m.Position);
        }

        async Task IDiscordAccess.SendWelcomeMessage(DiscordUserID userID)
        {
            var guildUser = GetGuildUserById(userID);
            var message = await _messageProvider.GetMessage(Constants.MessageNames.FirstServerJoinWelcome).ConfigureAwait(false);
            var privateChannel = await guildUser.GetOrCreateDMChannelAsync().ConfigureAwait(false);
            try
            {
                await privateChannel.SendMessageAsync(message).ConfigureAwait(false);
            }
            catch (HttpException e) when (e.DiscordCode == 50007)
            {
                _logger.LogDebug($"Couldn't send welcome message to '{guildUser.Username}', because private messages are probably blocked.");
            }
            catch (Exception e)
            {
                _logger.LogError($"Couldn't send welcome message to '{guildUser.Username}' due to an unexpected error: {e}");
            }
        }

        string IDiscordAccess.GetRoleMention(string roleName)
        {
            return GetRoleByName(roleName).Mention;
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
#pragma warning disable CS4014 // Fire & forget
            Task.Run(async () =>
            {
                if (!_userStore.IsInitialized)
                {
                    // Initialize user store only once
                    await _userStore.Initialize(guild.Users.Select(m => ((DiscordUserID)m.Id, SocketRoleToRole(m.Roles))).ToArray()).ConfigureAwait(false);
                }
                
                if (_gameRoleProvider.Games.Count == 0)
                {
                    // Load games only once
                    await _gameRoleProvider.LoadAvailableGames().ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
#pragma warning restore CS4014 // Fire & forget

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
            if (guildUser.Guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;

            _discordUserEventHandler.HandleJoined((DiscordUserID)guildUser.Id, SocketRoleToRole(guildUser.Roles));
            return Task.CompletedTask;
        }

        private Task Client_UserLeft(SocketGuildUser guildUser)
        {
            if (guildUser.Guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;

            _discordUserEventHandler.HandleLeft((DiscordUserID) guildUser.Id);
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
                // Handle possible role change
                var oldRoles = SocketRoleToRole(oldGuildUser.Roles);
                var newRoles = SocketRoleToRole(newGuildUser.Roles);
                if (oldRoles != newRoles)
                {
                    await UpdateGuildUserRoles((DiscordUserID)newGuildUser.Id, oldRoles, newRoles).ConfigureAwait(false);
                }

                // Handle possible status change
                if (oldGuildUser.Status != newGuildUser.Status)
                {
                    var wasOnline = IsOnline(oldGuildUser);
                    var isOnline = IsOnline(newGuildUser);
                    await _discordUserEventHandler.HandleStatusChanged((DiscordUserID)newGuildUser.Id, wasOnline, isOnline).ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        private async Task Client_MessageReceived(SocketMessage message)
        {
            // If the message is not a user message, we don't need to handle it
            SocketUserMessage userMessage;
            if ((userMessage = message as SocketUserMessage) == null) return;

            // If the message is from this bot, or any other bot, we don't need to handle it
            if (userMessage.Author.Id == _client.CurrentUser.Id || userMessage.Author.IsBot) return;
            
            // Check for spam
            if (await IsSpam(userMessage).ConfigureAwait(false)) return;
            
            // If the message is no spam, process message
            await ProcessMessage(userMessage).ConfigureAwait(false);
        }

        #endregion
    }
}