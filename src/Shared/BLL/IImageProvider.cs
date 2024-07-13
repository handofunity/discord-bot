namespace HoU.GuildBot.Shared.BLL;

public interface IImageProvider
{
    Stream CreateAocClassDistributionImage();

    Stream CreateAocPlayStyleDistributionImage();

    Stream CreateAocRaceDistributionImage();

    Stream CreateAocGuildPreferenceDistributionImage();

    Stream LoadLaunchRosterImage();

    Task<Stream> CreateProfileCardImage(DiscordUserId userID,
                                        string avatarUrl,
                                        ProfileInfoResponse profileData,
                                        string guildTag,
                                        bool hasPvpRole,
                                        bool hasArtisanRole,
                                        bool hasPveRole);

    Stream CreateLeaderboardTable(DiscordLeaderboardResponse leaderboardData);

    Stream LoadClassListImage();

    Stream LoadArtisanProfessionsImage();

    Stream CreateLostArkPlayStyleDistributionImage();

    Stream CreateWowRetailPlayStyleDistributionImage();

    Stream CreateAocRolePreferenceDistributionImage();

    Stream CreateTnlRolePreferenceDistributionImage();

    Stream CreateTnlWeaponDistributionImage();
}