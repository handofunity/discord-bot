namespace HoU.GuildBot.Shared.BLL
{
    using System.IO;

    public interface IImageProvider
    {
        Stream CreateAocRolesImage();
    }
}