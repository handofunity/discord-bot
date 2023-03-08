namespace HoU.GuildBot.Keycloak.Objects.Internal;

internal record KeycloakUserGroupAggregation(ConfiguredKeycloakGroups ConfiguredKeycloakGroups,
                                             UserRepresentation[] AllKeycloakUsers,
                                             IReadOnlyDictionary<KeycloakUserId, DiscordUserId> KeycloakToDiscordUserMapping,
                                             IDictionary<KeycloakGroupId, KeycloakUserId[]> KeycloakGroupMembers,
                                             IReadOnlyList<KeycloakUserId> DisabledUserIds);