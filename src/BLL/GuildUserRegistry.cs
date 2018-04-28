namespace HoU.GuildBot.BLL
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.Enums;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GuildUserRegistry : IGuildUserRegistry
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<GuildUserRegistry> _logger;
        private readonly ConcurrentDictionary<ulong, Role> _guildUserRoles;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildUserRegistry(ILogger<GuildUserRegistry> logger)
        {
            _logger = logger;
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

        void IGuildUserRegistry.AddGuildUsers(IEnumerable<(ulong UserId, Role Roles)> guildUsers)
        {
            foreach (var (userId, roles) in guildUsers)
            {
                _guildUserRoles.AddOrUpdate(userId, uid => roles, (uid, oldRoles) => roles);
            }
        }

        void IGuildUserRegistry.RemoveGuildUsers(IEnumerable<ulong> userIds)
        {
            foreach (var uid in userIds)
            {
                RemoveGuildUser(uid);
            }
        }

        void IGuildUserRegistry.AddGuildUser(ulong userId, Role roles)
        {
            _guildUserRoles.AddOrUpdate(userId, uid => roles, (uid, oldRoles) => roles);
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
                       : Role.Undefined;
        }

        #endregion
    }
}