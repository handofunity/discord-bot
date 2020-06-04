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
        private readonly IUnitsSyncService _unitsSyncService;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildMembersModule(IGuildInfoProvider guildInfoProvider,
                                  IUnitsSyncService unitsSyncService)
        {
            _guildInfoProvider = guildInfoProvider;
            _unitsSyncService = unitsSyncService;
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

        [Command("sync-units")]
        [CommandCategory(CommandCategory.MemberInformation, 2)]
        [Name("Sync with UNITS")]
        [Summary("Manually syncs the guild members with the UNIT system")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Leader | Role.Officer)]
        public async Task SyncUnitsAsync()
        {
            await _unitsSyncService.SyncAllUsers().ConfigureAwait(false);
        }

        #endregion
    }
}