using global::Discord;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.DAL.Discord
{
    public static class Extensions
    {
        internal static Embed ToEmbed(this EmbedData ed)
        {
            if (ed == null)
                return null;
            var builder = new EmbedBuilder();
            if (ed.Author != null)
                builder.Author = new EmbedAuthorBuilder
                {
                    Name = ed.Author,
                    Url = ed.AuthorUrl,
                    IconUrl = ed.AuthorIconUrl
                };
            if (ed.ThumbnailUrl != null)
                builder.ThumbnailUrl = ed.ThumbnailUrl;
            if (ed.Title != null)
                builder.Title = ed.Title;
            if (ed.Url != null)
                builder.Url = ed.Url;
            if (ed.Color != null)
                builder.Color = new Color(ed.Color.Value.R, ed.Color.Value.G, ed.Color.Value.B);
            if (ed.Description != null)
                builder.Description = ed.Description;
            if (ed.Fields != null)
            {
                foreach (var fd in ed.Fields)
                {
                    builder.AddField(fieldBuilder => fieldBuilder.WithName(fd.Name)
                                                                 .WithValue(fd.Value ?? "<null>")
                                                                 .WithIsInline(fd.Inline));
                }
            }
            if (ed.FooterText != null)
                builder.Footer.Text = ed.FooterText;
            if (ed.Timestamp != null)
                builder.Timestamp = ed.Timestamp;

            return builder.Build();
        }
    }
}