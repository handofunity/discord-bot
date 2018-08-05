namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.Enums;
    using Shared.StrongTypes;

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
            var data = _helpProvider.GetHelp((DiscordUserID)Context.User.Id, helpRequest);
            foreach (var t in data)
            {
                var embed = t.EmbedData.ToEmbed();
                await ReplyPrivateAsync(t.Message, embed).ConfigureAwait(false);
            }
        }

        #endregion
    }
}