namespace HoU.GuildBot.Shared.Enums
{
    public enum ResponseType
    {
        Undefined = 0,
        /// <summary>
        /// The response to commands with this <see cref="ResponseType"/> will always respond on a private channel (direct message from bot).
        /// </summary>
        AlwaysPrivate = 1,
        /// <summary>
        /// The response to commands with this <see cref="ResponseType"/> will always respond on the same channel the command was executed on (either private or public).
        /// </summary>
        AlwaysSameChannel = 2,
        /// <summary>
        /// The response to commands with this <see cref="ResponseType"/> will respond to multiple channels.
        /// </summary>
        MultipleChannels = 3
    }
}