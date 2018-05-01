namespace HoU.GuildBot.Shared.Objects
{
    public class GuildMemberPromotionResult
    {
        public bool CanPromote { get; }

        public string NoPromotionReason { get; }

        public EmbedData AnnouncementData { get; }

        public string LogMessage { get; }

        public GuildMemberPromotionResult(string noPromotionReason)
        {
            CanPromote = false;
            NoPromotionReason = noPromotionReason;
        }

        public GuildMemberPromotionResult(EmbedData announcementData, string logMessage)
        {
            CanPromote = true;
            AnnouncementData = announcementData;
            LogMessage = logMessage;
        }
    }
}