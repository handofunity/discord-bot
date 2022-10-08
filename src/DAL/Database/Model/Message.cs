using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class Message
    {
        public int MessageId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
