namespace HoU.GuildBot.Shared.Objects;

public record UnitsEndpoint(Uri BaseAddress,
                            string ClientSecret,
                            bool ConnectToRestApi,
                            bool ConnectToNotificationHub)
    : AuthorizationEndpoint(new Uri(BaseAddress, "/bot-api/auth/token"), "GuildBot", ClientSecret);