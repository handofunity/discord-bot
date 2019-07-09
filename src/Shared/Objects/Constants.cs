namespace HoU.GuildBot.Shared.Objects
{
    using System;

    public static class Constants
    {
        public const string InvalidCommandUsageTitle = ":octagonal_sign: Invalid command usage";

        /// <summary>
        /// Gets the global action delay for bulk operations on the Discord API.
        /// With this delay, a single bulk operation can use up to 30 of the 120 operations per minute.
        /// This leaves 90 operations per minute for other bulk operations and single scope operations.
        /// </summary>
        public static readonly TimeSpan GlobalActionDelay = TimeSpan.FromSeconds(2);

        public static class RuntimeEnvironment
        {
            public const string Development = "Development";
            public const string Production = "Production";
        }

        public static class MessageNames
        {
            public const string FirstServerJoinWelcome = "FirstServerJoinWelcome";
            public const string AocRoleMenu = "AocRoleMenu";
            public const string WowRoleMenu = "WowRoleMenu";
            public const string GamesRolesMenu = "GamesRolesMenu";
        }

        public static class RoleNames
        {
            public const string LeaderRoleName = "Leader";
            public const string OfficerRoleName = "Officer";
        }

        public static class RoleMenuGameShortNames
        {
            public const string AshesOfCreation = "AoC";
            public const string WorldOfWarcraftClassic = "WC";
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

        public static class WowRoleEmojis
        {
            public const string Druid = "\uD83C\uDF33"; // :deciduous_tree:
            public const string Hunter = "\uD83C\uDFF9"; // :bow_and_arrow:
            public const string Mage = "\uD83C\uDF86"; // :fireworks:
            public const string Paladin = "\u2733"; // :eight_spoked_asterisk: 
            public const string Priest = "\uD83C\uDFE5"; // :hospital:
            public const string Rogue = "\uD83D\uDC65"; // :busts_in_silhouette:
            public const string Warlock = "\uD83D\uDD73"; // :hole:
            public const string Warrior = "\uD83D\uDEE1"; // :shield:
        }

        public static class GamesRolesEmojis
        {
            public const string Joystick = "\uD83D\uDD79"; // :joystick: 
        }

        public static class NonMemberRolesEmojis
        {
            public const string Wave = "\uD83D\uDC4B"; // :wave:
            public const string Thinking = "\uD83E\uDD14"; // :thinking:
            public const string GameInterestAshesOfCreation = "AshesofCreation";
            public const string GameInterestWorldOfWarcraftClassic = "WoWClassic";
            public const string GameInterestOath = "oath";
        }
    }
}