namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class FederatedIdentityRepresentation
{
    [JsonIgnore]
    internal const string DiscordIdentityProviderName = "discord";

    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; init; }

    [JsonPropertyName("userId")]
    public string UserId { get; init; }

    [JsonIgnore]
    public DiscordUserId DiscordUserId => (DiscordUserId)ulong.Parse(UserId);

    [JsonPropertyName("userName")]
    public string Username { get; init; }

    [JsonConstructor]
    public FederatedIdentityRepresentation()
    {
        
    }
    
    public FederatedIdentityRepresentation(DiscordUserId userId,
                                           string username)
    {
        IdentityProvider = DiscordIdentityProviderName;
        UserId = userId.ToString();
        Username = username;
    }
}