namespace HoU.GuildBot.Shared.BLL;

public interface IKeycloakDiscordComparer
{
    KeycloakDiscordDiff GetDiff(UserModel[] discordUsers,
                                Dictionary<KeycloakUserId, DiscordUserId> keycloakUsers,
                                ConfiguredKeycloakGroups configuredKeycloakGroups,
                                Dictionary<KeycloakGroupId, KeycloakUserId[]> keycloakGroupMembers,
                                KeycloakUserId[] disabledUserIds);
}