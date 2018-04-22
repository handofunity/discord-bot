namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using Objects;

    public interface IBotEngine
    {
        Task Run(AppSettings settings);
    }
}