namespace HoU.GuildBot.Shared.Extensions;

public static class StrongTypesExtensions
{
    /// <summary>
    /// Converts the <paramref name="userId"/> into the mention syntax.
    /// </summary>
    /// <param name="userId">The <see cref="DiscordUserId"/> to convert into the mention syntax.</param>
    /// <returns>A string that will cause a mention in Discord.</returns>
    public static string ToMention(this DiscordUserId userId) => $"<@{userId}>";

    /// <summary>
    /// Converts the <paramref name="roleId"/> into the mention syntax.
    /// </summary>
    /// <param name="roleId">The <see cref="DiscordRoleId"/> to convert into the mention syntax.</param>
    /// <returns>A string that will cause a mention in Discord.</returns>
    public static string ToMention(this DiscordRoleId roleId) => $"<@&{roleId}>";
}