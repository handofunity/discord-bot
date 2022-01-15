namespace HoU.GuildBot.Shared.Objects;

public class UserRolesChangedResult
{
    public bool IsPromotion { get; }

    public EmbedData? AnnouncementData { get; }

    public string? LogMessage { get; }

    /// <summary>
    /// Returns a new instance of the <see cref="UserRolesChangedResult"/> class indicating that the update was no promotion.
    /// </summary>
    public UserRolesChangedResult()
    {
        IsPromotion = false;
    }

    public UserRolesChangedResult(EmbedData announcementData, string logMessage)
    {
        IsPromotion = true;
        AnnouncementData = announcementData;
        LogMessage = logMessage;
    }
}