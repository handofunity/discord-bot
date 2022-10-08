namespace HoU.GuildBot.Shared.Objects;

public record KeycloakDiscordDiff((UserModel DiscordUser, KeycloakGroupId[] KeycloakGroupIds)[] UsersToAdd,
                                  KeycloakUserId[] UsersToDisable,
                                  KeycloakUserId[] UsersToEnable,
                                  Dictionary<KeycloakUserId, KeycloakGroupId[]> GroupsToAdd,
                                  Dictionary<KeycloakUserId, KeycloakGroupId[]> GroupsToRemove);