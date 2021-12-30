using System;

namespace HoU.GuildBot.Shared.Objects
{
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
            public const string AocClassMenu = "AocClassMenu";
            public const string AocPlayStyleMenu = "AocPlayStyleMenu";
            public const string AocRaceMenu = "AocRaceMenu";
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
            // Classes
            public static readonly EmojiDefinition Bard = new("\uD83C\uDFB5"); // :musical_note:
            public static readonly EmojiDefinition Cleric = new("\uD83C\uDFE5"); // :hospital:
            public static readonly EmojiDefinition Fighter = new("\u2694"); // :crossed_swords:
            public static readonly EmojiDefinition Mage = new("\uD83C\uDF86"); // :fireworks:
            public static readonly EmojiDefinition Ranger = new("\uD83C\uDFF9"); // :bow_and_arrow:
            public static readonly EmojiDefinition Rogue = new("\uD83D\uDC65"); // :busts_in_silhouette:
            public static readonly EmojiDefinition Summoner = new("\uD83D\uDC23"); // :hatching_chick:
            public static readonly EmojiDefinition Tank = new("\uD83D\uDEE1"); // :shield:

            // Play styles
            public static readonly EmojiDefinition PvP = new("PvP", 817781653245919242);
            public static readonly EmojiDefinition PvE = new("\uD83D\uDC32"); // :dragon_face:
            public static readonly EmojiDefinition Crafting = new("\uD83D\uDC8D"); // :ring:

            // Races
            public static readonly EmojiDefinition Kaelar = new("AoCKaelar", 818553906421301258);
            public static readonly EmojiDefinition Vaelune = new("AoCVaelune", 818553906111709184);
            public static readonly EmojiDefinition Empyrean = new("AoCEmpyrean", 818553906488541204);
            public static readonly EmojiDefinition Pyrai = new("AoCPyrai", 818553906204377160);
            public static readonly EmojiDefinition Renkai = new("AoCRenKai", 818554358961537035);
            public static readonly EmojiDefinition Vek = new("AoCVek", 818553905927290880);
            public static readonly EmojiDefinition Dunir = new("AoCDunir", 818553906304516116);
            public static readonly EmojiDefinition Nikua = new("AoCNikua", 818553906454986764);
            public static readonly EmojiDefinition Tulnar = new("AoCTulnar", 818556838828048475);
        }

        public static class WowRoleEmojis
        {
            public static readonly EmojiDefinition Druid = new("WOWDruid", 607933001107636264);
            public static readonly EmojiDefinition Hunter = new("WOWHunter", 607945148005089282);
            public static readonly EmojiDefinition Mage = new("WOWMage", 607933001199779872);
            public static readonly EmojiDefinition Paladin = new("WOWPaladin", 607933001095184384);
            public static readonly EmojiDefinition Priest = new("WOWPriest", 607933001250242580);
            public static readonly EmojiDefinition Rogue = new("WOWRogue", 607933000751251477);
            public static readonly EmojiDefinition Warlock = new("WOWWarlock", 607933001174745089);
            public static readonly EmojiDefinition Warrior = new("WOWWarrior", 607933001090727946);
        }
        
        public static class GamesRolesEmojis
        {
            public static readonly EmojiDefinition Joystick = new("\uD83D\uDD79"); // :joystick: 
        }

        public static class NonMemberRolesEmojis
        {
            public static readonly EmojiDefinition Wave = new("\uD83D\uDC4B"); // :wave:
            public static readonly EmojiDefinition Thinking = new("\uD83E\uDD14"); // :thinking:
        }
    }
}