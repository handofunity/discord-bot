namespace HoU.GuildBot.Shared.Objects
{
    public class EmbedData
    {
        public string Title { get; set; }

        public string Url { get; set; }

        public RGB? Color { get; set; }

        public string Description { get; set; }

        public EmbedField[] Fields { get; set; }
    }
}