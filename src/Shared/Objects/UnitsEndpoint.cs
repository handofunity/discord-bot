namespace HoU.GuildBot.Shared.Objects;

public record UnitsEndpoint(Uri BaseAddress,
                            bool ConnectToRestApi,
                            bool ConnectToNotificationHub,
                            KeycloakEndpoint KeycloakEndpoint);