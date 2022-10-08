namespace HoU.GuildBot.Shared.Objects;

public class UserModel
{
    private string _username;
    private IReadOnlyList<DiscordRoleId> _roles;

    /// <summary>
    /// Gets or sets the user's Discord ID.
    /// </summary>
    public DiscordUserId DiscordUserId { get; init; }

    /// <summary>
    /// Gets or sets the user's Discord username, not unique across the platform.
    /// </summary>
    /// <exception cref="ArgumentNullException">The property gets set to <b>null</b>.</exception>
    public string Username
    {
        get => _username;
        set => _username = value ?? throw new ArgumentNullException(nameof(value), $"{nameof(Username)} cannot be set to null.");
    }

    /// <summary>
    /// Gets or sets the user's 4-digit Discord tag.
    /// </summary>
    public short Discriminator { get; set; }

    /// <summary>
    /// Gets or sets the user's Discord avatar hash.
    /// </summary>
    public string? AvatarId { get; set; }

    /// <summary>
    /// Gets or sets the roles the user has.
    /// </summary>
    /// <exception cref="ArgumentNullException">The property gets set to <b>null</b>.</exception>
    public IReadOnlyList<DiscordRoleId> Roles
    {
        get => _roles;
        set => _roles = value ?? throw new ArgumentNullException(nameof(value), $"{nameof(Roles)} cannot be set to null.");
    }

    /// <summary>
    /// Initializes a new <see cref="UserModel"/> instance, setting <see cref="Username"/> to <see cref="string.Empty"/>.
    /// </summary>
    public UserModel()
    {
        _username = string.Empty;
        _roles = Array.Empty<DiscordRoleId>();
    }
}