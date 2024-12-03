using Discord;

namespace HoU.GuildBot.Shared.Extensions;

public static class ValueObjectsExtensions
{
    /// <summary>
    /// Converts the <paramref name="userId"/> into the mention syntax.
    /// </summary>
    /// <param name="userId">The <see cref="DiscordUserId"/> to convert into the mention syntax.</param>
    /// <returns>A string that will cause a mention in Discord.</returns>
    public static string ToMention(this DiscordUserId userId) => MentionUtils.MentionUser((ulong)userId);

    /// <summary>
    /// Converts the <paramref name="usersIds"/> into the mention syntax.
    /// </summary>
    /// <param name="userIds">The <see cref="DiscordUserId"/>s to convert into the mention syntax.</param>
    /// <returns>An array of strings that will cause a mention in Discord.</returns>
    public static string[] ToMentions(this IEnumerable<DiscordUserId> usersIds) => usersIds.Select(m => m.ToMention()).ToArray();

    /// <summary>
    /// Converts the <paramref name="roleId"/> into the mention syntax.
    /// </summary>
    /// <param name="roleId">The <see cref="DiscordRoleId"/> to convert into the mention syntax.</param>
    /// <returns>A string that will cause a mention in Discord.</returns>
    public static string ToMention(this DiscordRoleId roleId) => MentionUtils.MentionRole((ulong)roleId);
}