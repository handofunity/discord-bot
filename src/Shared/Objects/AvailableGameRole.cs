namespace HoU.GuildBot.Shared.Objects;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class AvailableGameRole
{
    public DiscordRoleId DiscordRoleId { get; init; }

    public string? DisplayName { get; set; }

    private string GetDebuggerDisplay() =>
        string.IsNullOrWhiteSpace(DisplayName)
            ? DiscordRoleId.ToString()
            : DisplayName;

    public AvailableGameRole Clone()
    {
        return new AvailableGameRole
        {
            DiscordRoleId = DiscordRoleId
        };
    }
}