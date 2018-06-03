﻿namespace HoU.GuildBot.DAL.Discord
{
    using global::Discord;
    using Shared.Objects;

    public static class Extensions
    {
        internal static Embed ToEmbed(this EmbedData ed)
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
    }
}