namespace HoU.GuildBot.BLL
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    [UsedImplicitly]
    public class BotEngine : IBotEngine
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public BotEngine(IDiscordAccess discordAccess)
        {
            _discordAccess = discordAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private async Task Connect(string botToken)
        {
            await _discordAccess.Connect(botToken, ConnectedHandler, DisconnectedHandler).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IBotEngine Members

        async Task IBotEngine.Run(BotEngineArguments arguments)
        {
            // Create connection to Discord
            await Connect(arguments.BotToken).ConfigureAwait(false);

            // Listen to calls
            await Task.Delay(-1).ConfigureAwait(false);
        }
        
        private static async Task ConnectedHandler()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Connected to Discord.");
            Console.ResetColor();
            await Task.CompletedTask;
        }

        private async Task DisconnectedHandler(string lastBotToken, Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Lost connection to Discord.");
            Console.ResetColor();
            if (exception != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
                Console.ResetColor();
            }

            // Try to reconnect
            await Connect(lastBotToken).ConfigureAwait(false);
        }

        #endregion
    }
}