namespace HoU.GuildBot.Shared.BLL;

public interface IBotEngine
{
    /// <summary>
    /// Runs the bot indefinitely, until the <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to stop the bot.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task Run(CancellationToken cancellationToken);
}