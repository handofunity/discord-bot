using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class UserBirthday
    {
        public int UserId { get; set; }
        public short Month { get; set; }
        public short Day { get; set; }

        public virtual User? User { get; set; } = null!;
    }
}
