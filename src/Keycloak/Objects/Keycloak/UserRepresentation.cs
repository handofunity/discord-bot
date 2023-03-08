namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class UserRepresentation
{
    private string? _userId;
    private FederatedIdentityRepresentation[]? _federatedIdentities;

    [JsonPropertyName("id")]
    public string? UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            KeycloakUserId = _userId is not null && Guid.TryParse(UserId, out var userIdGuid)
                                 ? (KeycloakUserId)userIdGuid
                                 : default;
        }
    }

    [JsonIgnore]
    public KeycloakUserId KeycloakUserId { get; private set; }

    [JsonPropertyName("username")]
    public string? Username { get; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; }

    [JsonPropertyName("federatedIdentities")]
    public FederatedIdentityRepresentation[]? FederatedIdentities
    {
        get => _federatedIdentities;
        private init
        {
            _federatedIdentities = value;
            var federatedIdentity = _federatedIdentities?.FirstOrDefault();
            DiscordUserId = federatedIdentity?.DiscordUserId ?? default;
        }
    }

    [JsonIgnore]
    public DiscordUserId DiscordUserId { get; private set; }

    [JsonPropertyName("attributes")]
    public AttributeMap? Attributes { get; }

    public UserRepresentation(UserModel userModel)
    {
        Username = $"{userModel.Username.ToLower()}#{userModel.Discriminator:D4}";
        Enabled = true;
        FirstName = userModel.Username;
        FederatedIdentities = new[]
        {
            new FederatedIdentityRepresentation(userModel.DiscordUserId, Username)
        };
        Attributes = new AttributeMap(userModel.AvatarId,
                                      userModel.Nickname,
                                      null);
    }

    internal UserUpdateRepresentation AsUpdateRepresentation() => new(this);
}