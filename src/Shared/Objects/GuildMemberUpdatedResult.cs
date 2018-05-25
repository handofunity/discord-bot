namespace HoU.GuildBot.Shared.Objects
{
    public class GuildMemberUpdatedResult
    {
        public bool IsPromotion { get; }

        public EmbedData AnnouncementData { get; }

        public string LogMessage { get; }

        /// <summary>
        /// Returns a new instance of the <see cref="GuildMemberUpdatedResult"/> class indicating that the update was no promotion.
        /// </summary>
        public GuildMemberUpdatedResult()
        {
            IsPromotion = false;
        }

        public GuildMemberUpdatedResult(EmbedData announcementData, string logMessage)
        {
            IsPromotion = true;
            AnnouncementData = announcementData;
            LogMessage = logMessage;
        }
    }
}