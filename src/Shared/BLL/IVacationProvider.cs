using System;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IVacationProvider
    {
        Task<string> AddVacation(DiscordUserID userID, DateTime start, DateTime end, string note);

        Task<string> DeleteVacation(DiscordUserID userID, DateTime start, DateTime end);

        Task<string> GetVacations();

        Task<string> GetVacations(DiscordUserID userID);

        Task<string> GetVacations(DateTime date);
    }
}