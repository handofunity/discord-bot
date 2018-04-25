namespace HoU.GuildBot.Shared.Enums
{
    using System;

    [Flags]
    public enum Role
    {
        Undefined = 0,
        Developer = 1,
        Leader = 2,
        Officer = 4,
        Member = 8,
        Recruit = 16
    }
}