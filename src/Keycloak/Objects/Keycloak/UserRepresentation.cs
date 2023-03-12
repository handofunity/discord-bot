namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class UserRepresentation
{
    private readonly List<FederatedIdentityRepresentation>? _federatedIdentities;
    private string? _userId;

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
    public string? Username { get; init; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; init; }

    [JsonIgnore]
    public IReadOnlyList<FederatedIdentityRepresentation>? FederatedIdentities => _federatedIdentities;

    [JsonIgnore]
    public DiscordUserId DiscordUserId { get; private set; }

    [JsonPropertyName("attributes")]
    public AttributeMap? Attributes { get; init; }

    [JsonConstructor]
    public UserRepresentation()
    {
        _federatedIdentities = new List<FederatedIdentityRepresentation>();
    }

    public UserRepresentation(UserModel userModel)
        : this()
    {
        Username = userModel.FullUsername.ToLower();
        Enabled = true;
        FirstName = userModel.FullUsername;
        AddFederatedIdentity(new FederatedIdentityRepresentation(userModel.DiscordUserId,
                                                                 userModel.FullUsername));
        Attributes = new AttributeMap(userModel.AvatarId,
                                      userModel.Nickname,
                                      null);
    }

    internal void AddFederatedIdentity(FederatedIdentityRepresentation federatedIdentityRepresentation)
    {
        _federatedIdentities.Add(federatedIdentityRepresentation);
        var federatedIdentity = _federatedIdentities?.FirstOrDefault();
        DiscordUserId = federatedIdentity?.DiscordUserId ?? default;
    }

    internal UserUpdateRepresentation AsUpdateRepresentation() => new(this);
}