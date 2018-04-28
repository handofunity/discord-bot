namespace HoU.GuildBot.DAL.Modules
{
    using System.Threading.Tasks;
    using Discord.Commands;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.Enums;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class HelpModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IHelpProvider _helpProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public HelpModule(IHelpProvider helpProvider)
        {
            _helpProvider = helpProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("help")]
        [Name("Get command help")]
        [Summary("Provides help for commands.")]
        [Remarks("If no further arguments are provided, this command will list all available commands.")]
        [Alias("?")]
        [RequireContext(ContextType.DM | ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysPrivate)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task HelpAsync([Remainder] string helpRequest = null)
        {
            var data = _helpProvider.GetHelp(Context.User.Id, helpRequest);
            var embed = BuildEmbedFromData(data.Embed);
            await ReplyPrivateAsync(data.Message, embed).ConfigureAwait(false);
        }

        #endregion
    }
}