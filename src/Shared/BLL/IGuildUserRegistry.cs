﻿namespace HoU.GuildBot.Shared.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DAL;
    using Enums;
    using Objects;
    using StrongTypes;

    public interface IGuildUserRegistry
    {
        /// <summary>
        /// Sets the <see cref="IDiscordAccess"/> instance.
        /// </summary>
        IDiscordAccess DiscordAccess { set; }

        /// <summary>
        /// Checks if the user with the given <paramref name="userID"/> is a guild member.
        /// </summary>
        /// <param name="userID">The ID of the user to check.</param>
        /// <returns><b>True</b>, if the user is a guild member, otherwise <b>false</b>.</returns>
        bool IsGuildMember(DiscordUserID userID);

        /// <summary>
        /// Adds a list of users to the registry.
        /// </summary>
        /// <param name="guildUsers">A list of guild users.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task AddGuildUsers((DiscordUserID UserId, Role Roles)[] guildUsers);

        /// <summary>
        /// Removes all users from registry.
        /// </summary>
        /// <param name="userIds">The IDs of the users to remove.</param>
        void RemoveGuildUsers(IEnumerable<DiscordUserID> userIds);

        /// <summary>
        /// Adds a new user to the registry.
        /// </summary>
        /// <param name="userId">The ID of the user who should be added.</param>
        /// <param name="roles">The roles of the added user.</param>
        /// <returns><b>True</b>, if the user was never in the registry, otherwise <b>false</b>.</returns>
        Task<bool> AddGuildUser(DiscordUserID userId, Role roles);

        /// <summary>
        /// Removes a user from the registry.
        /// </summary>
        /// <param name="userId">The ID of the user to remove.</param>
        void RemoveGuildUser(DiscordUserID userId);

        /// <summary>
        /// Updates a user in the registry.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="mention">The mention related to the <paramref name="userId"/>.</param>
        /// <param name="oldRoles">The previous roles.</param>
        /// <param name="newRoles">The updated roles.</param>
        /// <returns>A <see cref="GuildMemberUpdatedResult"/>.</returns>
        GuildMemberUpdatedResult UpdateGuildUser(DiscordUserID userId, string mention, Role oldRoles, Role newRoles);

        /// <summary>
        /// Gets all roles for a given <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The ID of the user to get the roles for.</param>
        /// <returns>The roles the user has, or <see cref="Role.NoRole"/>, if the user is not in the registry.</returns>
        Role GetGuildUserRoles(DiscordUserID userId);

        /// <summary>
        /// Gets data for all guild members.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="DiscordAccess"/> is <b>null</b>.</exception>
        /// <returns>Data for all guild members.</returns>
        EmbedData GetGuildMembers();

        /// <summary>
        /// Gets an array of all current guild members user ids.
        /// </summary>
        /// <returns>An array of user IDs.</returns>
        DiscordUserID[] GetGuildMemberUserIds();
    }
}