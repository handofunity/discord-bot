namespace HoU.GuildBot.Shared.Objects;

public class EmbedField
{
    public string Name { get; }

    public object? Value { get; }

    public bool Inline { get; }

    public EmbedField(string name, object value, bool inline)
    {
        Name = name;
        Value = value;
        Inline = inline;
    }
}