namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class AttributeMap
{
    [JsonPropertyName(KnownAttributes.DiscordAvatarId)]
    public string[]? DiscordAvatarId { get; set; }

    [JsonPropertyName(KnownAttributes.DeleteAfter)]
    public string[]? DeleteAfter { get; set; }

    public AttributeMap(string? discordAvatarId,
                        string? deleteAfter)
    {
        SetDiscordAvatarId(discordAvatarId);
        SetDeleteAfter(deleteAfter);
    }

    [JsonConstructor]
    public AttributeMap()
    {

    }

    public void SetDiscordAvatarId(string? discordAvatarId) => DiscordAvatarId = discordAvatarId is null ? null : [discordAvatarId];

    public void SetDeleteAfter(string? deleteAfter) => DeleteAfter = deleteAfter is null ? null : [deleteAfter];

    public static AttributeMap Empty() => new();
}