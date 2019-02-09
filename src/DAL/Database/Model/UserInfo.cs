using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class UserInfo
    {
        public int UserID { get; set; }
        public DateTime LastSeen { get; set; }

        public virtual User User { get; set; }
    }
}