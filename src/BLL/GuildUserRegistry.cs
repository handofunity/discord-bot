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

        private static bool HasGuildMemberRole(Role roles)
        {
            return (Role.AnyGuildMember & roles) != Role.NoRole;
        }

        private ulong[] GetGuildMembersUserIds()
        {
            return _guildUserRoles.Where(m => HasGuildMemberRole(m.Value)).Select(m => m.Key).ToArray();
        }

        private Role GetGuildUserRoles(ulong userId)
        {
            return _guildUserRoles.TryGetValue(userId, out var roles)
                       ? roles
                       : Role.NoRole;
        }

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

        bool IGuildUserRegistry.IsGuildMember(ulong userID)
        {
            var roles = GetGuildUserRoles(userID);
            return HasGuildMemberRole(roles);
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

        GuildMemberUpdatedResult IGuildUserRegistry.UpdateGuildUser(ulong userId, string mention, Role oldRoles, Role newRoles)
        {
            _guildUserRoles.AddOrUpdate(userId, uid => newRoles, (uid, currentRegisteredRoles) => newRoles);

            // Check if the role change was a promotion
            Role promotedTo;
            if (!oldRoles.HasFlag(Role.Recruit)
             && !oldRoles.HasFlag(Role.Member) // Demotion from Member to Recruit should never happen, but just in case
             && newRoles.HasFlag(Role.Recruit))
            {
                promotedTo = Role.Recruit;
            }
            else if (!oldRoles.HasFlag(Role.Member)
                  && newRoles.HasFlag(Role.Member))
            {
                promotedTo = Role.Member;
            }
            else
            {
                return new GuildMemberUpdatedResult();
            }

            // Return result for announcement and logging the promotion
            var description = $"Congratulations {mention}, you've been promoted to the rank **{promotedTo}**.";
            if (promotedTo == Role.Recruit)
                description += " Welcome aboard!";
            var a = new EmbedData
            {
                Title = "Promotion",
                Color = Colors.BrightBlue,
                Description = description
            };
            return new GuildMemberUpdatedResult(a, $"{mention} has been promoted to **{promotedTo}**.");
        }

        Role IGuildUserRegistry.GetGuildUserRoles(ulong userId) => GetGuildUserRoles(userId);

        EmbedData IGuildUserRegistry.GetGuildMembers()
        {
            if (_discordAccess == null)
                throw new InvalidOperationException($"{nameof(IGuildUserRegistry.DiscordAccess)} must be set.");

            var guildMembers = GetGuildMembersUserIds();
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

        ulong[] IGuildUserRegistry.GetGuildMemberUserIds() => GetGuildMembersUserIds();

        #endregion
    }
}