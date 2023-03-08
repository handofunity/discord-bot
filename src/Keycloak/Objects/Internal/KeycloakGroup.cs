namespace HoU.GuildBot.Keycloak.Objects.Internal;

public record KeycloakGroup(KeycloakGroupId KeycloakGroupId,
                            DiscordRoleId? DiscordRoleId,
                            bool IsFallbackGroup);