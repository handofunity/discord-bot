namespace HoU.GuildBot.Shared.BLL
{
    using System.IO;
    using System.Threading.Tasks;
    using StrongTypes;

    public interface IImageProvider
    {
        Stream CreateAocRolesImage();

        Task<Stream> CreateProfileImage(DiscordUserID userID,
                                        string avatarUrl);
    }
}