namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class SpamProtectedChannel
    {
        public decimal SpamProtectedChannelId { get; set; }
        public int SoftCap { get; set; }
        public int HardCap { get; set; }
    }
}
