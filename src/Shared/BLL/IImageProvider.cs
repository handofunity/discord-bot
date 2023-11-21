namespace HoU.GuildBot.Shared.BLL;

public interface IImageProvider
{
    Stream CreateAocClassDistributionImage();

    Stream CreateAocPlayStyleDistributionImage();

    Stream CreateAocRaceDistributionImage();

    Stream CreateAocGuildPreferenceDistributionImage();
    
    Stream LoadLaunchRosterImage();

    Task<Stream> CreateProfileImage(DiscordUserId userID,
                                    string avatarUrl);

    Stream LoadClassListImage();

    Stream LoadArtisanProfessionsImage();

    Stream CreateLostArkPlayStyleDistributionImage();
    
    Stream CreateWowRetailPlayStyleDistributionImage();
    
    Stream CreateAocRolePreferenceDistributionImage();
}