﻿namespace HoU.GuildBot.Shared.Objects;

public static class Constants
{
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
        public const string AocClassMenuMessage = "AocClassMenu";
        public const string AocPlayStyleMenuMessage = "AocPlayStyleMenu";
        public const string AocRaceMenuMessage = "AocRaceMenu";
        public const string AocRolePreferenceMenuMessage = "AocRolePreferenceMenu";
        public const string WowRoleMenuMessage = "WowRoleMenu";
        public const string WowRetailPlayStyleMenuMessage = "WowRetailPlayStyleMenuMessage";
        public const string LostArkPlayStyleMenuMessage = "LostArkPlayStyleMenu";
        public const string GamesRolesMenuMessage = "GamesRolesMenu";
        public const string FriendOrGuestMenuMessage = "FriendOrGuestMenu";
        public const string GameInterestMenuMessage = "GameInterestMenu";
        public const string TrialMemberInformationMessage = "TrialMemberInformation";
    }

    public static class RoleNames
    {
        public const string LeaderRoleName = "Leader";
        public const string OfficerRoleName = "Officer";
    }

    public static class FriendOrGuestMenu
    {
        public const string TnlFriendCustomId = "6F2CAD3E-25A0-4DF4-B7E3-9192D5122BAB";
        public const string TnlFriendDisplayName = "TnL - Friend";
        public const string FriendOfMemberCustomId = "f32eeb72-3c37-4ef7-bff2-8c95b04d790e";
        public const string FriendofMemberDisplayName = "Friend of Member";
        public const string GuestCustomId = "63ce7730-02ab-47a2-833a-f77886e3289e";
        public const string GuestDisplayName = "Guest";
    }

    public static class GameRoleMenu
    {
        public const string CustomId = "4084e3ca-9015-41fd-b831-1256ebefa685";
    }

    public static class GameInterestMenu
    {
        public const string CustomId = "14f238d9-c9b6-4a2d-beea-fda89d6f1fab";
    }

    public static class Menus
    {
        private static readonly IReadOnlyDictionary<string, string> _mapping;

        static Menus()
        {
            _mapping = new Dictionary<string, string>
            {
                {AocArchetypeMenu.CustomId, "AshesOfCreationPrimaryGameDiscordRoleId"},
                {AocPlayStyleMenu.CustomId, "AshesOfCreationPrimaryGameDiscordRoleId"},
                {AocRaceMenu.CustomId, "AshesOfCreationPrimaryGameDiscordRoleId"},
                {AocRolePreferenceMenu.CustomId, "AshesOfCreationPrimaryGameDiscordRoleId"},
                {WowClassMenu.CustomId, "WorldOfWarcraftPrimaryGameRoleId"},
                {WowRetailPlayStyleMenu.CustomId, "WorldOfWarcraftRetailPrimaryGameRoleId"},
                {LostArkPlayStyleMenu.CustomId, "LostArkPrimaryGameRoleId"}
            };
        }

        /// <summary>
        /// Tries to map the <paramref name="customId"/> to the <paramref name="primaryGameRoleIdConfigurationKey"/> of an official guild chapter.
        /// </summary>
        /// <param name="customId">The Id of the action component to check.</param>
        /// <param name="primaryGameRoleIdConfigurationKey">The configuration key associated with the <paramref name="customId"/>.
        /// Use this to look up values in <see cref="IDynamicConfiguration.DiscordMapping"/>.</param>
        /// <returns><b>True</b>, if the <paramref name="customId"/> is mapped, otherwise <b>false</b>.</returns>
        public static bool IsMappedToPrimaryGameRoleIdConfigurationKey(string customId,
                                                                       out string? primaryGameRoleIdConfigurationKey) =>
            _mapping.TryGetValue(customId, out primaryGameRoleIdConfigurationKey);
    }

    public static class AocArchetypeMenu
    {
        public const string CustomId = "52766d05-c4a4-4bf7-9462-62c842932c9a";

        public static IDictionary<string, string> GetOptions() =>
            new Dictionary<string, string>
            {
                {"bard", "Bard"},
                {"cleric", "Cleric"},
                {"fighter", "Fighter"},
                {"mage", "Mage"},
                {"ranger", "Ranger"},
                {"rogue", "Rogue"},
                {"summoner", "Summoner"},
                {"tank", "Tank"}
            };
    }

    public static class AocPlayStyleMenu
    {
        public const string CustomId = "77bb8163-90df-44c6-b413-90f0547ab3fa";

        public static IDictionary<string, string> GetOptions() =>
            new Dictionary<string, string>
            {
                {"pve", "PvE"},
                {"pvp", "PvP"},
                {"artisan", "Artisan"}
            };
    }

    public static class AocRaceMenu
    {
        public const string CustomId = "a70c81d3-c10a-41c3-a221-56d2219e1619";

        public static IDictionary<string, string> GetOptions() =>
            new Dictionary<string, string>
            {
                {"kaelar", "Kaelar"},
                {"vaelune", "Vaelune"},
                {"empyrean", "Empyrean"},
                {"pyrai", "Pyrai"},
                {"renkai", "Renkai"},
                {"vek", "Vek"},
                {"dunir", "Dunir"},
                {"nikua", "Nikua"},
                {"tulnar", "Tulnar"},
            };
    }

    public static class AocRolePreferenceMenu
    {
        public const string CustomId = "1992dd22-df4a-4d75-957c-c1a4b885c70d";

        public static IDictionary<string, string> GetOptions() =>
            new Dictionary<string, string>
            {
                { "tank", "Tank" },
                { "support", "Support" },
                { "damagedealer", "Damage Dealer" }
            };
    }

    public static class WowClassMenu
    {
        public const string CustomId = "8edb79d3-86bf-4e6d-897e-ca94e97e0d4d";

        public static IDictionary<string, string> GetOptions() =>
            new Dictionary<string, string>
            {
                {"tank", "Tank"},
                {"healer", "Healer"},
                {"dps", "DPS"}
            };
    }

    public static class WowRetailPlayStyleMenu
    {
        public const string CustomId = "1f9ef663-d535-4824-9573-efee37e5b71e";

        public static IDictionary<string, string> GetOptions() =>
            new Dictionary<string, string>
            {
                { "tank", "Tank" },
                { "healer", "Healer" },
                { "meleedps", "Melee DPS" },
                { "rangeddps", "Ranged DPS" }
            };
    }

    public static class LostArkPlayStyleMenu
    {
        public const string CustomId = "BEA37582-5B00-4784-AC0E-2BBAE4500260";

        public static IDictionary<string, string> GetOptions() =>
            new Dictionary<string, string>
            {
                {"dps", "DPS"},
                {"support", "Support"},
                {"thirain-server", "Thirain Server"}
            };
    }
}