namespace HoU.GuildBot.Shared.DAL;

public interface IDiscordLogger
{
    /// <summary>
    /// Logs the <paramref name="message"/> in the dedicated logging channel on Discord.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <b>null</b>.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is empty or only whitespaces.</exception>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task LogToDiscordAsync(string message);
}