namespace HoU.GuildBot.Shared.Extensions
{
    using StrongTypes;

    public static class StrongTypesExtensions
    {
        /// <summary>
        /// Converts the <paramref name="userID"/> into the mention syntax.
        /// </summary>
        /// <param name="userID">The <see cref="DiscordUserID"/> to convert into the mention syntax.</param>
        /// <returns>A string that will cause a mention in Discord.</returns>
        public static string ToMention(this DiscordUserID userID) => $"<@{userID}>";
    }
}