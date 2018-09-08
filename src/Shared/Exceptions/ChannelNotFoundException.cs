namespace HoU.GuildBot.Shared.Exceptions
{
    using System;
    using StrongTypes;

    public class ChannelNotFoundException : Exception
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties

        public DiscordChannelID ChannelId { get; }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public ChannelNotFoundException(DiscordChannelID channelId) : base($"Couldn't find channel with the ID '{channelId}'.")
        {
            ChannelId = channelId;
        }

        #endregion
    }
}