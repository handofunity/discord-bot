namespace HoU.GuildBot.BLL
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly]
    public class GameRoleProvider : IGameRoleProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IDatabaseAccess _databaseAccess;
        private readonly ILogger<GameRoleProvider> _logger;
        private readonly List<AvailableGame> _games;

        private IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GameRoleProvider(IDatabaseAccess databaseAccess,
                                ILogger<GameRoleProvider> logger)
        {
            _databaseAccess = databaseAccess;
            _logger = logger;
            _games = new List<AvailableGame>();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGameRoleProvider Members

        IDiscordAccess IGameRoleProvider.DiscordAccess
        {
            set => _discordAccess = value;
        }

        public IReadOnlyList<AvailableGame> Games => _games;

        async Task<(bool Success, string Response, string LogMessage)> IGameRoleProvider.SetGameRole((DiscordUserID UserID, string Mention) user, AvailableGame game, string className)
        {
            var canChangeRole = _discordAccess.CanManageRolesForUser(user.UserID);
            if (!canChangeRole)
                return (false, $"{user.Mention}: The bot is not allowed to change your role.", null);

            if (game.ClassNames.All(m => m != className))
                return (false, $"Class name '{className}' is not valid for {game}.", null);
            // Game roles can be revoked before assigning the new one, as no channel permissions are tied to those
            await _discordAccess.RevokeCurrentGameRoles(user.UserID, game).ConfigureAwait(false);
            await _discordAccess.SetCurrentGameRole(user.UserID, game, className).ConfigureAwait(false);
            return (true, $"Class for {game} successfully changed to '{className}'.", $"{user.Mention} changed his class for {game} to {className}.");
        }

        async Task IGameRoleProvider.LoadAvailableGames()
        {
            _games.Clear();
            var games = await _databaseAccess.GetAvailableGames().ConfigureAwait(false);
            _games.AddRange(games);
            foreach (var availableGame in _games)
                _discordAccess.GetClassNamesForGame(availableGame);
            _logger.LogInformation($"Loaded {_games.Count} games with a total count of {_games.Sum(m => m.ClassNames.Count)} roles.");
        }

        #endregion
    }
}