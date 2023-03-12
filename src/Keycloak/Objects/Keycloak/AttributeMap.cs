namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class AttributeMap
{
    [JsonPropertyName(KnownAttributes.DiscordAvatarId)]
    public string[]? DiscordAvatarId { get; set; }

    [JsonPropertyName(KnownAttributes.DiscordNickname)]
    public string[]? DiscordNickname { get; set; }

    [JsonPropertyName(KnownAttributes.DeleteAfter)]
    public string[]? DeleteAfter { get; set; }

    public AttributeMap(string? discordAvatarId,
                        string? discordNickname,
                        string? deleteAfter)
    {
        SetDiscordAvatarId(discordAvatarId);
        SetDiscordNickname(discordNickname);
        SetDeleteAfter(deleteAfter);
    }

    [JsonConstructor]
    public AttributeMap()
    {
        
    }

    public void SetDiscordAvatarId(string? discordAvatarId) => DiscordAvatarId = discordAvatarId is null ? null : new[] { discordAvatarId };
    
    public void SetDiscordNickname(string? discordNickname) => DiscordNickname = discordNickname is null ? null : new[] { discordNickname };
    
    public void SetDeleteAfter(string? deleteAfter) => DeleteAfter = deleteAfter is null ? null : new[] { deleteAfter };

    public static AttributeMap Empty() => new();
}