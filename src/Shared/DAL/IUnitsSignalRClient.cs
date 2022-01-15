using HoU.GuildBot.Shared.Objects;
using System.Threading.Tasks;

namespace HoU.GuildBot.Shared.DAL;

public interface IUnitsSignalRClient
{
    /// <summary>
    /// Connects to the UNITS hub system to receive push notifications.
    /// </summary>
    /// <param name="unitsEndpoint">The data used to sync with the UNIT system.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task ConnectAsync(UnitsEndpoint unitsEndpoint);
}