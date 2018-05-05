namespace HoU.GuildBot.BLL
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GuildUserRegistry : IGuildUserRegistry
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<GuildUserRegistry> _logger;
        private readonly IDatabaseAccess _databaseAccess;
        private readonly ConcurrentDictionary<ulong, Role> _guildUserRoles;

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

        async Task IGuildUserRegistry.AddGuildUsers((ulong UserId, Role Roles)[] guildUsers)
        {
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

        #endregion
    }
}