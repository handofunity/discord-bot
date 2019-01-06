namespace HoU.GuildBot.Shared.BLL
{
    public interface ITimeInformationProvider
    {
        string[] GetCurrentTimeFormattedForConfiguredTimezones();
    }
}