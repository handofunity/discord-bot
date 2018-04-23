namespace HoU.GuildBot.Shared.Objects
{
    public class EmbedData
    {
        public string Title { get; set; }

        public (byte R, byte G, byte B)? Color { get; set; }

        public EmbedField[] Fields { get; set; }
    }
}