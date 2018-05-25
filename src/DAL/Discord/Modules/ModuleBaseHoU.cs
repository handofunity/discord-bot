namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System.Threading.Tasks;
    using global::Discord;
    using global::Discord.Commands;

    public abstract class ModuleBaseHoU : ModuleBase<SocketCommandContext>
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Protected Methods

        protected async Task ReplyPrivateAsync(string message, Embed embed = null)
        {
            if (Context.IsPrivate)
            {
                await ReplyAsync(message, false, embed).ConfigureAwait(false);
            }
            else
            {
                var privateChannel = await Context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await privateChannel.SendMessageAsync(message, false, embed).ConfigureAwait(false);
            }
        }

        #endregion
    }
}