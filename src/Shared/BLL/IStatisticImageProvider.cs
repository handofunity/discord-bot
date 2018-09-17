namespace HoU.GuildBot.Shared.BLL
{
    using System.IO;

    public interface IStatisticImageProvider
    {
        Stream CreateAocRolesImage();
    }
}