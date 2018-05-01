namespace HoU.GuildBot.DAL.Modules
{
    using System.Threading.Tasks;
    using Discord.Commands;
    using Discord.WebSocket;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Objects;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PromoteModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGuildUserPromoter _guildUserPromoter;
        private readonly IDiscordAccess _discordAccess;
        private readonly AppSettings _appSettings;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public PromoteModule(IGuildUserPromoter guildUserPromoter,
                             IDiscordAccess discordAccess,
                             AppSettings appSettings)
        {
            _guildUserPromoter = guildUserPromoter;
            _discordAccess = discordAccess;
            _appSettings = appSettings;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("promote")]
        [Name("Promote a user")]
        [Summary("Promotes the mentioned user to a higher rank.")]
        [Remarks("Users can be promoted up to the rank one below the current user. If successful, the promotion will be announced.")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.MultipleChannels)]
        [RolePrecondition(Role.Leader | Role.Officer)]
        public async Task PromoteAsync(SocketGuildUser guildUser)
        {
            var result = await _guildUserPromoter.TryPromote((Context.User.Id, Context.User.Mention), (guildUser.Id, guildUser.Mention)).ConfigureAwait(false);
            if (result.CanPromote)
            {
                // Log promotion
                await _discordAccess.Log(result.LogMessage).ConfigureAwait(false);

                // Announce promotion
                var g = Context.Guild.GetTextChannel(_appSettings.PromotionAnnouncementChannelId);
                var embed = BuildEmbedFromData(result.AnnouncementData);
                await g.SendMessageAsync(string.Empty, false, embed).ConfigureAwait(false);
            }
            else
            {
                // Respond in the requesting channel, why the promotion cannot be executed
                await ReplyAsync(result.NoPromotionReason).ConfigureAwait(false);
            }
        }

        #endregion
    }
}