namespace HoU.GuildBot.Shared.ValueObjects;

[ValueObject<ulong>]
public readonly partial struct DiscordUserId
{
    public static readonly DiscordUserId Unknown = new(0);
}
