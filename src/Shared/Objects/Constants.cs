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
            public static readonly EmojiDefinition Bard = new EmojiDefinition("\uD83C\uDFB5"); // :musical_note:
            public static readonly EmojiDefinition Cleric = new EmojiDefinition("\uD83C\uDFE5"); // :hospital:
            public static readonly EmojiDefinition Fighter = new EmojiDefinition("\u2694"); // :crossed_swords:
            public static readonly EmojiDefinition Mage = new EmojiDefinition("\uD83C\uDF86"); // :fireworks:
            public static readonly EmojiDefinition Ranger = new EmojiDefinition("\uD83C\uDFF9"); // :bow_and_arrow:
            public static readonly EmojiDefinition Rogue = new EmojiDefinition("\uD83D\uDC65"); // :busts_in_silhouette:
            public static readonly EmojiDefinition Summoner = new EmojiDefinition("\uD83D\uDC23"); // :hatching_chick:
            public static readonly EmojiDefinition Tank = new EmojiDefinition("\uD83D\uDEE1"); // :shield:
        }

        public static class WowRoleEmojis
        {
            public static readonly EmojiDefinition Druid = new EmojiDefinition("WOWDruid", 607933001107636264);
            public static readonly EmojiDefinition Hunter = new EmojiDefinition("WOWHunter", 607945148005089282);
            public static readonly EmojiDefinition Mage = new EmojiDefinition("WOWMage", 607933001199779872);
            public static readonly EmojiDefinition Paladin = new EmojiDefinition("WOWPaladin", 607933001095184384);
            public static readonly EmojiDefinition Priest = new EmojiDefinition("WOWPriest", 607933001250242580);
            public static readonly EmojiDefinition Rogue = new EmojiDefinition("WOWRogue", 607933000751251477);
            public static readonly EmojiDefinition Warlock = new EmojiDefinition("WOWWarlock", 607933001174745089);
            public static readonly EmojiDefinition Warrior = new EmojiDefinition("WOWWarrior", 607933001090727946);
        }

        public static class GamesRolesEmojis
        {
            public static readonly EmojiDefinition Joystick = new EmojiDefinition("\uD83D\uDD79"); // :joystick: 
        }

        public static class NonMemberRolesEmojis
        {
            public static readonly EmojiDefinition Wave = new EmojiDefinition("\uD83D\uDC4B"); // :wave:
            public static readonly EmojiDefinition Thinking = new EmojiDefinition("\uD83E\uDD14"); // :thinking:
            public static readonly EmojiDefinition GameInterestAshesOfCreation = new EmojiDefinition("AshesOfCreation", null);
            public static readonly EmojiDefinition GameInterestWorldOfWarcraftClassic = new EmojiDefinition("WoWClassic", null);
            public static readonly EmojiDefinition GameInterestOath = new EmojiDefinition("Oath", null);
            public static readonly EmojiDefinition GameInterestShopTitans = new EmojiDefinition("ShopTitans", null);
            public static readonly EmojiDefinition GameInterestFinalFantasy14 = new EmojiDefinition("FinalFantasy", null);
        }

        //public static class OtherEmojis
        //{
        //    public static readonly EmojiDefinition
        //}
    }
}