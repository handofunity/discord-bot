using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.DAL
{
    public interface IUnityHubAccess
    {
        /// <summary>
        /// Gets all valid role names that can be synced to the system.
        /// </summary>
        /// <returns>An array of allowed role names in the Unity Hub.</returns>
        string[] GetValidRoleNames();

        /// <summary>
        /// Sends the <paramref name="users"/> to the Unity Hub.
        /// </summary>
        /// <param name="users">The users to synchronize.</param>
        /// <returns><b>True</b>, if the request was successful, otherwise <b>false</b>.</returns>
        Task<bool> SendAllUsersAsync(UserModel[] users);
    }
}