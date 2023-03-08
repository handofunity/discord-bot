namespace HoU.GuildBot.Shared.Objects;

public class UserModel
{
    /// <summary>
    /// Gets or sets the user's Discord ID.
    /// </summary>
    public required DiscordUserId DiscordUserId { get; init; }

    /// <summary>
    /// Gets or sets the user's Discord username, not unique across the platform.
    /// </summary>
    /// <exception cref="ArgumentNullException">The property gets set to <b>null</b>.</exception>
    public required string Username { get; init; }

    /// <summary>
    /// Gets or sets the user's server-specific Nickname.
    /// </summary>
    public required string? Nickname { get; init; }

    /// <summary>
    /// Gets or sets the user's 4-digit Discord tag.
    /// </summary>
    public required short Discriminator { get; init; }

    /// <summary>
    /// Gets or sets the user's (global or server-specific) Discord avatar hash.
    /// </summary>
    public required string? AvatarId { get; set; }

    /// <summary>
    /// Gets or sets the roles the user has.
    /// </summary>
    /// <exception cref="ArgumentNullException">The property gets set to <b>null</b>.</exception>
    public required IReadOnlyList<DiscordRoleId> Roles { get; set; }
}