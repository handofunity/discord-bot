using System;

namespace HoU.GuildBot.Shared.Objects
{
    public class EmbedData
    {
        public string Author { get; set; }

        public string AuthorUrl { get; set; }

        public string AuthorIconUrl { get; set; }

        public string ThumbnailUrl { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public RGB? Color { get; set; }

        public string Description { get; set; }

        public EmbedField[] Fields { get; set; }

        public string FooterText { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
    }
}