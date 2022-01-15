using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects;

public class AvailableGameRole
{
    public DiscordRoleId DiscordRoleId { get; init; }

    public string? DisplayName { get; set; }

    public AvailableGameRole Clone()
    {
        return new AvailableGameRole
        {
            DiscordRoleId = DiscordRoleId
        };
    }
}