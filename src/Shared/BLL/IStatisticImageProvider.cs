namespace HoU.GuildBot.Shared.BLL
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IStatisticImageProvider
    {
        Task<Stream> CreateAocRolesImage();
    }
}