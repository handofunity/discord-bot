namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Objects;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GuildUserRegistry : IGuildUserRegistry
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<GuildUserRegistry> _logger;
        private readonly IDatabaseAccess _databaseAccess;
        private readonly ConcurrentDictionary<ulong, Role> _guildUserRoles;
        private IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildUserRegistry(ILogger<GuildUserRegistry> logger,
                                 IDatabaseAccess databaseAccess)
        {
            _logger = logger;
            _databaseAccess = databaseAccess;
            _guildUserRoles = new ConcurrentDictionary<ulong, Role>();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private void RemoveGuildUser(ulong userId)
        {
            var removed = _guildUserRoles.TryRemove(userId, out _);
            if (!removed)
                _logger.LogWarning($"Couldn't remove user from registry with ID '{userId}'.");
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGuildUserRegistry Members

        IDiscordAccess IGuildUserRegistry.DiscordAccess
        {
            set => _discordAccess = value;
        }

        async Task IGuildUserRegistry.AddGuildUsers((ulong UserId, Role Roles)[] guildUsers)
        {
            _logger.LogInformation($"Adding {guildUsers.Length} users to the registry.");
            foreach (var (userId, roles) in guildUsers)
            {
                _guildUserRoles.AddOrUpdate(userId, uid => roles, (uid, oldRoles) => roles);
            }

            // Check against database
            await _databaseAccess.AddUsers(guildUsers.Select(m => m.UserId)).ConfigureAwait(false);
        }

        void IGuildUserRegistry.RemoveGuildUsers(IEnumerable<ulong> userIds)
        {
            foreach (var uid in userIds)
            {
                RemoveGuildUser(uid);
            }
        }

        async Task<bool> IGuildUserRegistry.AddGuildUser(ulong userId, Role roles)
        {
            _guildUserRoles.AddOrUpdate(userId, uid => roles, (uid, oldRoles) => roles);
            // Check against database
            return await _databaseAccess.AddUser(userId).ConfigureAwait(false);
        }

        void IGuildUserRegistry.RemoveGuildUser(ulong userId)
        {
            RemoveGuildUser(userId);
        }

        void IGuildUserRegistry.UpdateGuildUser(ulong userId, Role roles)
        {
            _guildUserRoles.AddOrUpdate(userId, uid => roles, (uid, oldRoles) => roles);
        }

        Role IGuildUserRegistry.GetGuildUserRoles(ulong userId)
        {
            return _guildUserRoles.TryGetValue(userId, out var roles)
                       ? roles
                       : Role.NoRole;
        }

        EmbedData IGuildUserRegistry.GetGuildMembers()
        {
            if (_discordAccess == null)
                throw new InvalidOperationException($"{nameof(IGuildUserRegistry.DiscordAccess)} must be set.");

            var guildMembers = _guildUserRoles.Where(m => (Role.AnyGuildMember & m.Value) != Role.NoRole).Select(m => m.Key).ToArray();
            var total = guildMembers.Length;
            var online = guildMembers.Count(guildMember => _discordAccess.IsUserOnline(guildMember));

            return new EmbedData
            {
                Title = "Guild members",
                Color = Colors.LightGreen,
                Fields = new []
                {
                    new EmbedField("Total", total.ToString(), true), 
                    new EmbedField("Online", online.ToString(), true) 
                }
            };
        }

        #endregion
    }
}