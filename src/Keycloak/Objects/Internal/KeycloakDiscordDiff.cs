namespace HoU.GuildBot.Shared.Objects;

internal record KeycloakDiscordDiff((UserModel DiscordUser, KeycloakGroupId[] KeycloakGroupIds)[] UsersToAdd,
                                    UserRepresentation[] UsersToDisable,
                                    UserRepresentation[] UsersToEnable,
                                    IReadOnlyList<(UserModel DiscordState, UserRepresentation KeycloakState)> UsersWithDifferentProperties,
                                    IReadOnlyDictionary<KeycloakUserId, UserModel> UsersWithDifferentIdentityData,
                                    Dictionary<KeycloakUserId, KeycloakGroupId[]> GroupsToAdd,
                                    IReadOnlyDictionary<KeycloakUserId, KeycloakGroupId[]> GroupsToRemove);