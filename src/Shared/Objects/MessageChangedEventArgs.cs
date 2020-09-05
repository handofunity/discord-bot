using System;

namespace HoU.GuildBot.Shared.Objects
{
    public class MessageChangedEventArgs : EventArgs
    {
        public string MessageName { get; }

        public MessageChangedEventArgs(string messageName)
        {
            MessageName = messageName;
        }
    }
}