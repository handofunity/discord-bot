namespace HoU.GuildBot.DAL
{
    using System;
    using System.Threading.Tasks;
    using Discord;
    using Discord.WebSocket;
    using JetBrains.Annotations;
    using Shared.DAL;

    [UsedImplicitly]
    public class DiscordAccess : IDiscordAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly DiscordSocketClient _client;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DiscordAccess()
        {
            _client = new DiscordSocketClient();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDiscordAccess Members

        async Task IDiscordAccess.Connect(string botToken, Func<Task> connectedHandler, Func<string, Exception, Task> disconnectedHandler)
        {
            if (botToken == null)
                throw new ArgumentNullException(nameof(botToken));
            if (connectedHandler == null)
                throw new ArgumentNullException(nameof(connectedHandler));
            if (disconnectedHandler == null)
                throw new ArgumentNullException(nameof(disconnectedHandler));
            if (string.IsNullOrWhiteSpace(botToken))
                throw new ArgumentException(nameof(botToken));

            try
            {
                _client.Connected += () =>
                {
                    _client.MessageReceived += Client_MessageReceived;
                    return connectedHandler();
                };
                _client.Disconnected += exception =>
                {
                    _client.MessageReceived -= Client_MessageReceived;
                    return disconnectedHandler(botToken, exception);
                };

                await _client.LoginAsync(TokenType.Bot, botToken).ConfigureAwait(false);
                await _client.StartAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Event Handler

        private static async Task Client_MessageReceived(SocketMessage message)
        {
            if (message.Content == "hou!ping")
            {
                await message.Channel.SendMessageAsync("Pong!").ConfigureAwait(false);
            }
        }

        #endregion
    }
}