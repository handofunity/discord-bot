namespace HoU.GuildBot.Shared.Exceptions
{
    using System;

    public class ChannelNotFoundException : Exception
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties

        public ulong ChannelId { get; }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public ChannelNotFoundException(ulong channelId) : base($"Couldn't find channel with the ID '{channelId}'.")
        {
            ChannelId = channelId;
        }

        #endregion
    }
}