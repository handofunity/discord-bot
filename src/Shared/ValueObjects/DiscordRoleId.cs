namespace HoU.GuildBot.Shared.ValueObjects;

[ValueObject<ulong>]
public readonly partial struct DiscordRoleId
{
    public static readonly DiscordRoleId Unknown = new(0);
}
