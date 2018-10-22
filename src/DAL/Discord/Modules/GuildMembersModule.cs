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
    public class GuildMembersModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGuildInfoProvider _guildInfoProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildMembersModule(IGuildInfoProvider guildInfoProvider)
        {
            _guildInfoProvider = guildInfoProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("guildmembers")]
        [CommandCategory(CommandCategory.MemberInformation, 1)]
        [Name("List guild members count")]
        [Summary("Lists the count of guild members")]
        [Alias("guild members")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task GuildMembersAsync()
        {
            var data = _guildInfoProvider.GetGuildMemberStatus();
            var embed = data.ToEmbed();
            await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
        }

        #endregion
    }
}