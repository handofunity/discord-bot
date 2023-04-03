namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class FederatedIdentityRepresentation
{
    [JsonIgnore]
    internal const string DiscordIdentityProviderName = "discord";

    /// <remarks><b>Required</b> for creating a new user.</remarks>
    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; init; }

    /// <remarks><b>Required</b> for creating a new user.</remarks>
    [JsonPropertyName("userId")]
    public string UserId { get; init; }

    [JsonIgnore]
    public DiscordUserId DiscordUserId => (DiscordUserId)ulong.Parse(UserId);

    /// <remarks><b>Required</b> for creating a new user.</remarks>
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