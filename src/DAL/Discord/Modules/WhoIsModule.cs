namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using global::Discord.WebSocket;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.Enums;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class WhoIsModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserInfoProvider _userInfoProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public WhoIsModule(IUserInfoProvider userInfoProvider)
        {
            _userInfoProvider = userInfoProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("whois")]
        [CommandCategory(CommandCategory.Help, 2)]
        [Name("Gets \"who is\" user info by mention")]
        [Summary("Gets internal information about the given user.")]
        [Alias("who is")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer)]
        public async Task WhoIsAsync(SocketGuildUser whoIsUser)
        {
            var data = _userInfoProvider.WhoIs((DiscordUserID)whoIsUser.Id);
            var embed = data.ToEmbed();
            await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
        }

        [Command("whois")]
        [CommandCategory(CommandCategory.Help, 3)]
        [Name("Gets \"who is\" user info by InternalUserID or DiscordUserID")]
        [Summary("Gets internal information about the given user ID.")]
        [Alias("who is")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer)]
        public async Task WhoIsByIdAsync([Remainder] string remainder)
        {
            var data = _userInfoProvider.WhoIs(Context.User.Username, remainder);
            var embed = data.ToEmbed();
            await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
        }

        #endregion
    }
}