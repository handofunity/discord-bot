namespace HoU.GuildBot.BLL
{
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;

    [UsedImplicitly]
    public class GameRoleProvider : IGameRoleProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GameRoleProvider(IDiscordAccess discordAccess)
        {
            _discordAccess = discordAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGameRoleProvider Members

        async Task<(bool Success, string Response, string LogMessage)> IGameRoleProvider.SetGameRole((ulong UserID, string Mention) user, Game game, string className)
        {
            var canChangeRole = _discordAccess.CanManageRolesForUser(user.UserID);
            if (!canChangeRole)
                return (false, $"{user.Mention}: The bot is not allowed to change your role.", null);

            var gameRoles = _discordAccess.GetClassNamesForGame(game);
            if (gameRoles.All(m => m != className))
                return (false, $"Class name '{className}' is not valid for {game}.", null);
            // Game roles can be revoked before assigning the new one, as no channel permissions are tied to those
            await _discordAccess.RevokeCurrentGameRoles(user.UserID, game).ConfigureAwait(false);
            await _discordAccess.SetCurrentGameRole(user.UserID, game, className).ConfigureAwait(false);
            return (true, $"Class for {game} successfully changed to '{className}'.", $"{user.Mention} changed his class for {game} to {className}.");
        }

        #endregion
    }
}