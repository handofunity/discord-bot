namespace HoU.GuildBot.DAL.Modules
{
    using Discord;
    using Discord.Commands;
    using Shared.Objects;

    public abstract class ModuleBaseHoU : ModuleBase<SocketCommandContext>
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Protected Methods

        protected static Embed BuildEmbedFromData(EmbedData ed)
        {
            var builder = new EmbedBuilder();
            if (ed.Title != null)
                builder.Title = ed.Title;
            if (ed.Color != null)
                builder.Color = new Color(ed.Color.Value.R, ed.Color.Value.G, ed.Color.Value.B);
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

        #endregion
    }
}