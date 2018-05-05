namespace HoU.GuildBot.Shared.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DAL;
    using Enums;
    using Objects;

    public interface IGuildUserRegistry
    {
        /// <summary>
        /// Sets the <see cref="IDiscordAccess"/> instance.
        /// </summary>
        IDiscordAccess DiscordAccess { set; }

        /// <summary>
        /// Adds a list of users to the registry.
        /// </summary>
        /// <param name="guildUsers">A list of guild users.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task AddGuildUsers((ulong UserId, Role Roles)[] guildUsers);

        /// <summary>
        /// Removes all users from registry.
        /// </summary>
        /// <param name="userIds">The IDs of the users to remove.</param>
        void RemoveGuildUsers(IEnumerable<ulong> userIds);

        /// <summary>
        /// Adds a new user to the registry.
        /// </summary>
        /// <param name="userId">The ID of the user who should be added.</param>
        /// <param name="roles">The roles of the added user.</param>
        /// <returns><b>True</b>, if the user was never in the registry, otherwise <b>false</b>.</returns>
        Task<bool> AddGuildUser(ulong userId, Role roles);

        /// <summary>
        /// Removes a user from the registry.
        /// </summary>
        /// <param name="userId">The ID of the user to remove.</param>
        void RemoveGuildUser(ulong userId);

        /// <summary>
        /// Updates a user in the registry.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="roles">The updated roles.</param>
        void UpdateGuildUser(ulong userId, Role roles);

        /// <summary>
        /// Gets all roles for a given <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The ID of the user to get the roles for.</param>
        /// <returns>The roles the user has, or <see cref="Role.NoRole"/>, if the user is not in the registry.</returns>
        Role GetGuildUserRoles(ulong userId);

        /// <summary>
        /// Gets data for all guild members.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="DiscordAccess"/> is <b>null</b>.</exception>
        /// <returns>Data for all guild members.</returns>
        EmbedData GetGuildMembers();
    }
}