namespace HoU.GuildBot.Shared.DAL
{
    using System.Threading.Tasks;
    using Objects;

    public interface IUnitsAccess
    {
        /// <summary>
        /// Queries the UNIT system for all valid role names that can be synced to the system.
        /// </summary>
        /// <returns>An array of allowed role names in the UNIT system, or <b>null</b>, if the web request failed.</returns>
        Task<string[]> GetValidRoleNamesAsync();

        /// <summary>
        /// Sends the <paramref name="users"/> to the UNIT system.
        /// </summary>
        /// <param name="users">The users to synchronize.</param>
        /// <returns>A <see cref="SyncAllUsersResponse"/>, if the <paramref name="users"/> were synchronized, otherwise <b>null</b>.</returns>
        Task<SyncAllUsersResponse> SendAllUsersAsync(UserModel[] users);
    }
}