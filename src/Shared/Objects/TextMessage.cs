using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects
{
    public class TextMessage
    {
        /// <summary>
        /// Gets the ID of the channel the text message was send in.
        /// </summary>
        public DiscordChannelID ChannelID { get; }

        /// <summary>
        /// Gets the ID of the text message.
        /// </summary>
        public ulong MessageID { get; }

        /// <summary>
        /// Gets the content of the text message.
        /// </summary>
        public string Content { get; }

        public TextMessage(DiscordChannelID channelID,
                           ulong messageID,
                           string content)
        {
            ChannelID = channelID;
            MessageID = messageID;
            Content = content;
        }
    }
}