namespace HoU.GuildBot.Shared.Objects;

public record AuthorizationEndpoint(Uri AccessTokenUrl,
                                    string ClientId,
                                    string ClientSecret)
{
    public string AccessTokenBaseAddress { get; } = AccessTokenUrl.GetLeftPart(UriPartial.Authority);

    public string AccessTokenRoute { get; } = AccessTokenUrl.PathAndQuery;
}