namespace HoU.GuildBot.Shared.Enums
{
    using System;

    [Flags]
    public enum Role
    {
        NoRole = 0,
        /// <summary>
        /// Only users with the role <see cref="Developer"/> may use these commands.
        /// </summary>
        Developer = 1,
        /// <summary>
        /// Only users with the role <see cref="Leader"/> may use these commands.
        /// </summary>
        Leader = 2,
        /// <summary>
        /// Only users with the role <see cref="Officer"/> may use these commands.
        /// </summary>
        Officer = 4,
        /// <summary>
        /// Only users with the role <see cref="Coordinator"/> may use these commands.
        /// </summary>
        Coordinator = 8,
        /// <summary>
        /// Only users with the role <see cref="Member"/> may use these commands.
        /// </summary>
        Member = 16,
        /// <summary>
        /// Only users with the role <see cref="Recruit"/> may use these commands.
        /// </summary>
        Recruit = 32,
        /// <summary>
        /// Any user who is a guild member may use these commands.
        /// </summary>
        AnyGuildMember = Leader | Officer | Coordinator | Member | Recruit,
        /// <summary>
        /// Not a guild member, but a <see cref="Guest"/> role.
        /// </summary>
        Guest = 64,
        /// <summary>
        /// Not a guild member, but a <see cref="FriendOfMember"/> role.
        /// </summary>
        FriendOfMember = 128,
        /// <summary>
        /// Not a guild member, but interest playing Ashes of Creation.
        /// </summary>
        GameInterestAshesOfCreation = 256,
        /// <summary>
        /// Not a guild member, but interest playing World of Warcraft Classic.
        /// </summary>
        GameInterestWorldOfWarcraftClassic = 512,
        /// <summary>
        /// Not a guild member, but interest playing Oath.
        /// </summary>
        GameInterestOath = 1024,
        /// <summary>
        /// Not a guild member, but interest playing Shop Titans.
        /// </summary>
        GameInterestShopTitans = 2048,
        /// <summary>
        /// Not a guild member, but interest playing Final Fantasy XIV.
        /// </summary>
        GameInterestFinalFantasy14 = 4096
    }
}