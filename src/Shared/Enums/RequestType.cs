namespace HoU.GuildBot.Shared.Enums
{
    using System;

    [Flags]
    public enum RequestType
    {
        Undefined = 0,
        GuildChannel = 1,
        DirectMessage = 2
    }
}