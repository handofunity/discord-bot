namespace HoU.GuildBot.Shared.Objects;

public record KeycloakSyncResponse(int AddedUsers,
                                   int DisabledUsers,
                                   int LoggedOutUsers,
                                   int EnabledUsers,
                                   int AssignedGroupMemberships,
                                   int RemovedGroupMemberships);