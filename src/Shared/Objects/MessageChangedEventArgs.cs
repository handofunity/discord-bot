namespace HoU.GuildBot.Shared.Objects
{
    using System;

    public class MessageChangedEventArgs : EventArgs
    {
        public string MessageName { get; }

        public MessageChangedEventArgs(string messageName)
        {
            MessageName = messageName;
        }
    }
}