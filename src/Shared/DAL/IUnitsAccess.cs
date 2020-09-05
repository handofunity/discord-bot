﻿using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.DAL
{
    public interface IUnitsAccess
    {
        /// <summary>
        /// Queries the UNIT system for all valid role names that can be synced to the system.
        /// </summary>
        /// <param name="unitsSyncData">The data used to sync with the UNIT system.</param>
        /// <returns>An array of allowed role names in the UNIT system, or <b>null</b>, if the web request failed.</returns>
        Task<string[]> GetValidRoleNamesAsync(UnitsSyncData unitsSyncData);

        /// <summary>
        /// Sends the <paramref name="users"/> to the UNIT system.
        /// </summary>
        /// <param name="unitsSyncData">The data used to sync with the UNIT system.</param>
        /// <param name="users">The users to synchronize.</param>
        /// <returns>A <see cref="SyncAllUsersResponse"/>, if the <paramref name="users"/> were synchronized, otherwise <b>null</b>.</returns>
        Task<SyncAllUsersResponse> SendAllUsersAsync(UnitsSyncData unitsSyncData,
                                                     UserModel[] users);
    }
}