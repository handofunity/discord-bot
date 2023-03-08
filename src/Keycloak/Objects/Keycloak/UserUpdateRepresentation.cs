namespace HoU.GuildBot.Keycloak.Objects.Keycloak;

internal class UserUpdateRepresentation
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }
    
    [JsonPropertyName("attributes")]
    public AttributeMap Attributes { get; }
    
    internal UserUpdateRepresentation(UserRepresentation userRepresentation)
    {
        Enabled = userRepresentation.Enabled;
        Attributes = userRepresentation.Attributes ?? AttributeMap.Empty();
    }
    
    internal JsonNode ToJson() => JsonSerializer.SerializeToNode(this);
}