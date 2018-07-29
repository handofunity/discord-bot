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
    public class IgnoreModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IIgnoreGuard _ignoreGuard;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public IgnoreModule(IIgnoreGuard ignoreGuard)
        {
            _ignoreGuard = ignoreGuard;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("ignore me")]
        [Name("Ignore commands")]
        [Summary("Requests the bot instance to ignore further commands from the user for a certain duration.")]
        [Remarks("The minimum ignore duration is 3 minutes, the maximum ignore duration is 60 minutes.")]
        [Alias("ignoreme")]
        [RequireContext(ContextType.DM | ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysPrivate)]
        [RolePrecondition(Role.Developer)]
        public async Task IgnoreMeAsync([Remainder] string remainder)
        {
            var result = _ignoreGuard.TryAddToIgnoreList((DiscordUserID)Context.User.Id, Context.User.Username, remainder);
            var embed = result.ToEmbed();
            await ReplyPrivateAsync(string.Empty, embed).ConfigureAwait(false);
        }

        [Command("notice me")]
        [Name("Stop ignoring commands")]
        [Summary("Requests the bot instance to accept commands from the current user again.")]
        [Remarks("If the user was not ignored by the bot, he won't receive any message. The bot will only respond during the ignored time.")]
        [Alias("noticeme")]
        [RequireContext(ContextType.DM | ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysPrivate)]
        [RolePrecondition(Role.Developer)]
        public async Task NoticeMeAsync()
        {
            var result = _ignoreGuard.TryRemoveFromIgnoreList((DiscordUserID)Context.User.Id, Context.User.Username);
            if (result != null)
            {
                var embed = result.ToEmbed();
                await ReplyPrivateAsync(string.Empty, embed).ConfigureAwait(false);
            }
        }

        #endregion
    }
}