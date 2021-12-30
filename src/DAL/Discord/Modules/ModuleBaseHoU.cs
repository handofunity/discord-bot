using System.Threading.Tasks;
using global::Discord;
using global::Discord.Commands;

namespace HoU.GuildBot.DAL.Discord.Modules
{
    public abstract class ModuleBaseHoU : ModuleBase<SocketCommandContext>
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Protected Methods

        protected async Task ReplyPrivateAsync(string message, Embed embed = null)
        {
            if (Context.IsPrivate)
            {
                await ReplyAsync(message, false, embed);
            }
            else
            {
                var privateChannel = await Context.User.CreateDMChannelAsync();
                await privateChannel.SendMessageAsync(message, false, embed);
            }
        }

        #endregion
    }
}