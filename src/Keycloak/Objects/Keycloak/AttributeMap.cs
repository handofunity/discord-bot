namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class AttributeMap
{
    [JsonPropertyName(KnownAttributes.DiscordAvatarId)]
    public string[]? DiscordAvatarId { get; private set; }

    [JsonPropertyName(KnownAttributes.DiscordNickname)]
    public string[]? DiscordNickname { get; private set; }

    [JsonPropertyName(KnownAttributes.DeleteAfter)]
    public string[]? DeleteAfter { get; private set; }

    public AttributeMap(string? discordAvatarId,
                        string? discordNickname,
                        string? deleteAfter)
    {
        SetDiscordAvatarId(discordAvatarId);
        SetDiscordNickname(discordNickname);
        SetDeleteAfter(deleteAfter);
    }

    private AttributeMap()
    {
        
    }

    public void SetDiscordAvatarId(string? discordAvatarId) => DiscordAvatarId = discordAvatarId is null ? null : new[] { discordAvatarId };
    
    public void SetDiscordNickname(string? discordNickname) => DiscordNickname = discordNickname is null ? null : new[] { discordNickname };
    
    public void SetDeleteAfter(string? deleteAfter) => DeleteAfter = deleteAfter is null ? null : new[] { deleteAfter };

    public static AttributeMap Empty() => new();
}