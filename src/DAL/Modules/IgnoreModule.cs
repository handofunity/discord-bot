namespace HoU.GuildBot.DAL.Modules
{
    using System.Threading.Tasks;
    using Discord.Commands;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.BLL;
    using Shared.Enums;

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
        [RolePrecondition(Role.Developer)]
        public async Task IgnoreMeAsync([Remainder] string remainder)
        {
            var result = _ignoreGuard.TryAddToIgnoreList(Context.User.Id, Context.User.Username, remainder);
            var embed = BuildEmbedFromData(result);
            await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
        }

        [Command("notice me")]
        [RolePrecondition(Role.Developer)]
        public async Task NoticeMeAsync()
        {
            var result = _ignoreGuard.TryRemoveFromIgnoreList(Context.User.Id, Context.User.Username);
            if (result != null)
            {
                var embed = BuildEmbedFromData(result);
                await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
            }
        }

        #endregion
    }
}