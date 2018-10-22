namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.Enums;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class InfoModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IBotInformationProvider _botInformationProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public InfoModule(IBotInformationProvider botInformationProvider)
        {
            _botInformationProvider = botInformationProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("info")]
        [CommandCategory(CommandCategory.Administration, 1)]
        [Name("Get bot information")]
        [Summary("Gets information about the current bot instance.")]
        [Alias("information")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task InfoAsync()
        {
            var data = _botInformationProvider.GetData();
            var embed = data.ToEmbed();
            await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
        }

        #endregion
    }
}