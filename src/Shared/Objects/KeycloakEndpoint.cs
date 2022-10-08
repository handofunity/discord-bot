namespace HoU.GuildBot.Shared.Objects;

public record KeycloakEndpoint(Uri BaseUrl,
                               Uri AccessTokenUrl,
                               string ClientId,
                               string ClientSecret,
                               string Realm)
    : AuthorizationEndpoint(AccessTokenUrl, ClientId, ClientSecret);