using System.Diagnostics.CodeAnalysis;

namespace HoU.GuildBot.Shared.Extensions;

public static class StringExtensions
{
    [return: NotNullIfNotNull(nameof(name))]
    public static string? SanitizeName(this string? name,
        bool spaceAllowed)
    {
        if (name is null)
            return null;

        if (!spaceAllowed)
            name = name.Replace(" ", "");

        return name.Replace("~", "")
            .Replace("#", "")
            .Replace("=", "")
            .Replace("?", "")
            .Replace("!", "")
            .Replace("|", "")
            .Replace("^", "")
            .Replace("$", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("/", "")
            .Replace("  ", " ")
            .Trim();
    }
}
