namespace HoU.GuildBot.DAL.Modules
{
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Shared.Objects;

    public abstract class ModuleBaseHoU : ModuleBase<SocketCommandContext>
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Protected Methods

        protected static Embed BuildEmbedFromData(EmbedData ed)
        {
            if (ed == null)
                return null;
            var builder = new EmbedBuilder();
            if (ed.Title != null)
                builder.Title = ed.Title;
            if (ed.Color != null)
                builder.Color = new Color(ed.Color.Value.R, ed.Color.Value.G, ed.Color.Value.B);
            if (ed.Description != null)
                builder.Description = ed.Description;
            if (ed.Fields != null)
            {
                foreach (var fd in ed.Fields)
                {
                    builder.AddField(fieldBuilder => fieldBuilder.WithName(fd.Name)
                                                                 .WithValue(fd.Value)
                                                                 .WithIsInline(fd.Inline));
                }
            }

            return builder.Build();
        }

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