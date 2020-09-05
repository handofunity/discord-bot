using System;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Exceptions
{
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