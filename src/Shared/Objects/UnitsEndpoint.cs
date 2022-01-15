namespace HoU.GuildBot.Shared.Objects;

public record UnitsEndpoint(string BaseAddress,
                            string Secret,
                            bool ConnectToRestApi,
                            bool ConnectToNotificationHub);