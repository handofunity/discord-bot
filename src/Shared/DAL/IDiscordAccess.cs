namespace HoU.GuildBot.Shared.DAL
{
    using System;
    using System.Threading.Tasks;

    public interface IDiscordAccess
    {
        /// <summary>
        /// Tries to establish a connection to Discord.
        /// </summary>
        /// <param name="connectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection has been established.</param>
        /// <param name="disconnectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection has been lost.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectedHandler"/> or <paramref name="disconnectedHandler"/> are <b>null</b>.</exception>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Connect(Func<Task> connectedHandler, Func<Exception, Task> disconnectedHandler);

        /// <summary>
        /// Sets the <paramref name="gameName"/> as current bot game name.
        /// </summary>
        /// <param name="gameName">The game name.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SetCurrentGame(string gameName);
    }
}