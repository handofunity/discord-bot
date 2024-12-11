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
                : KeycloakUserId.Unknown;
        }
    }

    [JsonIgnore]
    public KeycloakUserId KeycloakUserId { get; private set; }

    /// <remarks><b>Required</b> for creating a new user.</remarks>
    [JsonPropertyName("username")]
    public string Username { get; init; }

    /// <remarks><b>Required</b> for creating a new user.</remarks>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <remarks><b>Required</b> for creating a new user.</remarks>
    [JsonPropertyName("firstName")]
    public string FirstName { get; init; }

    /// <remarks><b>Optional</b> for creating a new user.</remarks>
    [JsonPropertyName("lastName")]
    public string? LastName { get; init; }

    /// <remarks><b>Required</b> for creating a new user.</remarks>
    [JsonPropertyName("federatedIdentities")]
    public IReadOnlyList<FederatedIdentityRepresentation>? FederatedIdentities => _federatedIdentities;

    [JsonIgnore]
    public DiscordUserId DiscordUserId { get; private set; }

    [JsonPropertyName("attributes")]
    public AttributeMap? Attributes { get; init; }

    [JsonConstructor]
    public UserRepresentation()
    {
        _federatedIdentities = [];
        DiscordUserId = DiscordUserId.Unknown;
    }

    public UserRepresentation(UserModel userModel)
        : this()
    {
        Username = userModel.Username.ToLower();
        Enabled = true;
        FirstName = userModel.GlobalName;
        LastName = userModel.Nickname;
        AddFederatedIdentity(new FederatedIdentityRepresentation(userModel.DiscordUserId,
                                                                 userModel.Username));
        Attributes = new AttributeMap(userModel.AvatarId,
                                      null);
    }

    internal void AddFederatedIdentity(FederatedIdentityRepresentation federatedIdentityRepresentation)
    {
        _federatedIdentities!.Add(federatedIdentityRepresentation);
        var federatedIdentity = _federatedIdentities!.FirstOrDefault();
        DiscordUserId = federatedIdentity?.DiscordUserId ?? DiscordUserId.Unknown;
    }

    internal UserUpdateRepresentation AsUpdateRepresentation() => new(this);
}