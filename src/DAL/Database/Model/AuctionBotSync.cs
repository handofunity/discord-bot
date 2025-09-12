namespace HoU.GuildBot.DAL.Database.Model;

public partial class AuctionBotSync
{
    public decimal DiscordUserId { get; set; }
    public DateTime LastChange { get; set; }
    public long HeritageTokens { get; set; }
}
