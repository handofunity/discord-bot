namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class FederatedIdentityRepresentation
{
    internal const string DiscordIdentityProviderName = "discord";

    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; }

    [JsonPropertyName("userId")]
    public string UserId { get; }

    [JsonIgnore]
    public DiscordUserId DiscordUserId => (DiscordUserId)ulong.Parse(UserId);

    [JsonPropertyName("userName")]
    public string Username { get; }

    public FederatedIdentityRepresentation(DiscordUserId userId,
                                           string username)
    {
        IdentityProvider = DiscordIdentityProviderName;
        UserId = userId.ToString();
        Username = username;
    }
}