namespace HoU.GuildBot.Shared.Objects;

public class EmbedData
{
    public string? Author { get; init; }

    public string? AuthorUrl { get; init; }

    public string? AuthorIconUrl { get; init; }

    public string? ThumbnailUrl { get; init; }

    public string? Title { get; init; }

    public string? Url { get; set; }

    public RGB? Color { get; init; }

    public string? Description { get; set; }

    public EmbedField[]? Fields { get; set; }

    public string? FooterText { get; set; }

    public string? ImageUrl { get; set; }
}