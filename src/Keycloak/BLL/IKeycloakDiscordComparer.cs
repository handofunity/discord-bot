namespace HoU.GuildBot.Keycloak.BLL;

internal interface IKeycloakDiscordComparer
{
    KeycloakDiscordDiff GetDiff(KeycloakUserGroupAggregation keycloakState,
                                UserModel[] discordUsers);
}