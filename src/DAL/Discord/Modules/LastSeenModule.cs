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
    public class LastSeenModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserInfoProvider _userInfoProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public LastSeenModule(IUserInfoProvider userInfoProvider)
        {
            _userInfoProvider = userInfoProvider;
        }

        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("last seen", RunMode = RunMode.Async)]
        [Name("Shows last seen timestamps")]
        [Summary("Lists all users and the timestamp of their last text message.")]
        [Remarks("List is ordered by descanding activity (those who are away the longest are on the top).")]
        [Alias("lastseen")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Leader | Role.SeniorOfficer)]
        public async Task LastSeenAsync()
        {
            var message = await ReplyAsync("Loading...").ConfigureAwait(false);

            var embedData = await _userInfoProvider.GetLastSeenInfo().ConfigureAwait(false);
            
            await message.DeleteAsync().ConfigureAwait(false);

            await ReplyAsync(string.Empty, false, embedData.ToEmbed()).ConfigureAwait(false);
        }

        #endregion
    }
}