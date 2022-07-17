namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class GameRole
    {
        public short GameRoleId { get; set; }
        public decimal DiscordRoleId { get; set; }
        public short GameId { get; set; }
        public int ModifiedByUserId { get; set; }
        public DateTime ModifiedAtTimestamp { get; set; }

        public virtual Game? Game { get; set; } = null!;
        public virtual User? ModifiedByUser { get; set; } = null!;
    }
}
