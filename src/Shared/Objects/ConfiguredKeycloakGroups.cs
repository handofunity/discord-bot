namespace HoU.GuildBot.Shared.Objects;

public record ConfiguredKeycloakGroups(IReadOnlyDictionary<DiscordRoleId, KeycloakGroupId> DiscordRoleToKeycloakGroupMapping,
                                       KeycloakGroupId FallbackGroupId);