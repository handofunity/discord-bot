namespace HoU.GuildBot.Shared.BLL
{
    using System;
    using System.Threading.Tasks;

    public interface IVacationProvider
    {
        Task<string> AddVacation(ulong userID, DateTime start, DateTime end, string note);

        Task<string> DeleteVacation(ulong userID, DateTime start, DateTime end);

        Task<string> GetVacations();

        Task<string> GetVacations(ulong userID);

        Task<string> GetVacations(DateTime date);
    }
}