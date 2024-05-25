namespace HoU.GuildBot.Shared.Objects;

public record UnitsEndpoint(int UnitsEndpointId,
                            Uri BaseAddress,
                            bool ConnectToRestApi,
                            bool ConnectToNotificationHub,
                            KeycloakEndpoint KeycloakEndpoint);