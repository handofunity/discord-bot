namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.StrongTypes;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GameRoleModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGameRoleProvider _gameRoleProvider;
        private readonly IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GameRoleModule(IGameRoleProvider gameRoleProvider,
                              IDiscordAccess discordAccess)
        {
            _gameRoleProvider = gameRoleProvider;
            _discordAccess = discordAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("set game role", RunMode = RunMode.Async)]
        [Name("Set a game related role")]
        [Summary("Sets a game related role for the calling user.")]
        [Remarks("Syntax: _set game role GAME CLASS_ e.g.: _set game role AoC Ranger_\r\n" +
                 "Only one role per game can be active. Any previous game related role will be revoked.")]
        [Alias("setgamerole")]
        [RequireContext(ContextType.DM |ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task SetGameRoleAsync([Remainder] string messageContent)
        {
            var regex = new Regex("^(?<game>\\w+) +(?<className>\\w+)$");
            var match = regex.Match(messageContent);
            if (!match.Success)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }

            var v = match.Groups["game"].Value;
            var game = _gameRoleProvider.Games.SingleOrDefault(m => m.ShortName == v || m.LongName == v);
            if (game == null)
            {
                await ReplyAsync($"{Context.User.Mention}: Couldn't find a matching game for '{v}'.").ConfigureAwait(false);
                return;
            }
            var className = match.Groups["className"].Value;

            var result = await _gameRoleProvider.SetGameRole(((DiscordUserID)Context.User.Id, Context.User.Mention), game, className).ConfigureAwait(false);
            if (result.Success)
            {
                // Log
                await _discordAccess.LogToDiscord(result.LogMessage).ConfigureAwait(false);

                // Send response
                await ReplyAsync(result.Response).ConfigureAwait(false);
            }
            else
            {
                // Send response
                await ReplyAsync(result.Response).ConfigureAwait(false);
            }
        }

        #endregion
    }
}