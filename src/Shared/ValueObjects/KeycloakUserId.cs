namespace HoU.GuildBot.Shared.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct KeycloakUserId
{
    public static readonly KeycloakUserId Unknown = new(Guid.Empty);
}
