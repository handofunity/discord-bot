using System;

namespace HoU.GuildBot.Shared.Enums
{
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
        /// Only users with the role <see cref="TrialMember"/> may use these commands.
        /// </summary>
        TrialMember = 32,
        /// <summary>
        /// Any user who is a guild member may use these commands.
        /// </summary>
        AnyGuildMember = Leader | Officer | Coordinator | Member | TrialMember,
        /// <summary>
        /// Not a guild member, but a <see cref="Guest"/> role.
        /// </summary>
        Guest = 64,
        /// <summary>
        /// Not a guild member, but a <see cref="FriendOfMember"/> role.
        /// </summary>
        FriendOfMember = 128
    }
}