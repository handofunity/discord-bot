namespace HoU.GuildBot.DAL.Modules
{
    using System.Threading.Tasks;
    using Discord.Commands;
    using JetBrains.Annotations;
    using Preconditions;
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
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task InfoAsync()
        {
            var data = _botInformationProvider.GetData();
            var embed = BuildEmbedFromData(data);
            await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
        }

        #endregion
    }
}