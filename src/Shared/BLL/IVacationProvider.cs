namespace HoU.GuildBot.Shared.BLL;

public interface IVacationProvider
{
    Task<string> AddVacation(DiscordUserId userID, DateTime start, DateTime end, string? note);

    Task<string> DeleteVacation(DiscordUserId userID, DateTime start, DateTime end);

    Task<string> GetVacations();

    Task<string> GetVacations(DiscordUserId userID);

    Task<string> GetVacations(DateTime date);
}