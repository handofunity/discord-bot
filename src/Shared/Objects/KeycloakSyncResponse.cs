namespace HoU.GuildBot.Shared.Objects;

public record KeycloakSyncResponse(int AddedUsers,
                                   int DisabledUsers,
                                   int LoggedOutUsers,
                                   int EnabledUsers,
                                   int UpdatedDetails,
                                   int AssignedGroupMemberships,
                                   int RemovedGroupMemberships);