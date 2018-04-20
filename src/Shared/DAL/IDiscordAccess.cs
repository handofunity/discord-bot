namespace HoU.GuildBot.Shared.DAL
{
    using System;
    using System.Threading.Tasks;

    public interface IDiscordAccess
    {
        /// <summary>
        /// Tries to establish a connection to Discord.
        /// </summary>
        /// <param name="botToken">The bot token used to connect to Discord.</param>
        /// <param name="connectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection has been established.</param>
        /// <param name="disconnectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection has been lost.</param>
        /// <exception cref="ArgumentNullException"><paramref name="botToken"/>, <paramref name="connectedHandler"/> or <paramref name="disconnectedHandler"/> are <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="botToken"/> is an empty string.</exception>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Connect(string botToken, Func<Task> connectedHandler, Func<string, Exception, Task> disconnectedHandler);

        /// <summary>
        /// Sets the <paramref name="gameName"/> as current bot game name.
        /// </summary>
        /// <param name="gameName">The game name.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SetCurrentGame(string gameName);
    }
}