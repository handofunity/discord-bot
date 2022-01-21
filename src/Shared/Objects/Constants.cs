using System;
using System.Collections.Generic;

namespace HoU.GuildBot.Shared.Objects;

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
        public const string AocClassMenu = "AocClassMenu";
        public const string AocPlayStyleMenu = "AocPlayStyleMenu";
        public const string AocRaceMenu = "AocRaceMenu";
        public const string WowRoleMenu = "WowRoleMenu";
        public const string GamesRolesMenu = "GamesRolesMenu";
        public const string FriendOrGuestMenu = "FriendOrGuestMenu";
        public const string GameInterestMenu = "GameInterestMenu";
    }

    public static class RoleNames
    {
        public const string LeaderRoleName = "Leader";
        public const string OfficerRoleName = "Officer";
    }

    public static class FriendOrGuestMenu
    {
        public const string FriendOfMemberCustomId = "f32eeb72-3c37-4ef7-bff2-8c95b04d790e";
        public const string GuestCustomId = "63ce7730-02ab-47a2-833a-f77886e3289e";

        public static IDictionary<string, string> GetOptions() =>
            new Dictionary<string, string>
            {
                { FriendOfMemberCustomId, "Friend of Member" },
                { GuestCustomId, "Guest" }
            };
    }

    public static class GameInterestMenu
    {
        public const string CustomId = "14f238d9-c9b6-4a2d-beea-fda89d6f1fab";
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
                {"crafting", "Crafting"}
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
}