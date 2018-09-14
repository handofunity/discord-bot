namespace HoU.GuildBot.Shared.Objects
{
    using System;

    public static class Constants
    {
        public const string InvalidCommandUsageTitle = ":octagonal_sign: Invalid command usage";

        /// <summary>
        /// Gets the global action delay for bulk operations on the Discord APU.
        /// With this delay, a single bulk operation can use up to 30 of the 120 operations per minute.
        /// This leaves 90 operations per minute for other bulk operations and single scope operations.
        /// </summary>
        public static readonly TimeSpan GlobalActionDelay = TimeSpan.FromSeconds(2);

        public static class MessageNames
        {
            public const string FirstServerJoinWelcome = "FirstServerJoinWelcome";
            public const string WelcomeChannelMessage01 = "WelcomeChannelMessage_01";
            public const string WelcomeChannelMessage02 = "WelcomeChannelMessage_02";
            public const string WelcomeChannelMessage03 = "WelcomeChannelMessage_03";
            public const string WelcomeChannelMessage04 = "WelcomeChannelMessage_04";
            public const string AocRoleMenu = "AocRoleMenu";
        }

        public static class RoleNames
        {
            public const string LeaderRoleName = "Leader";
            public const string SeniorOfficerRoleName = "Senior Officer";
        }

        public static class AocRoleEmojis
        {
            public const string Bard = "\uD83C\uDFB5"; // :musical_note:
            public const string Cleric = "\uD83C\uDFE5"; // :hospital:
            public const string Fighter = "\u2694"; // :crossed_swords:
            public const string Mage = "\uD83C\uDF86"; // :fireworks:
            public const string Ranger = "\uD83C\uDFF9"; // :bow_and_arrow:
            public const string Rogue = "\uD83D\uDC65"; // :busts_in_silhouette:
            public const string Summoner = "\uD83D\uDC23"; // :hatching_chick:
            public const string Tank = "\uD83D\uDEE1"; // :shield:
        }
    }
}