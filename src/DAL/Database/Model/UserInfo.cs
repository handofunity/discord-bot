using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class UserInfo
    {
        public int UserId { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime JoinedDate { get; set; }
        public string? CurrentRoles { get; set; }
        public DateOnly? PromotedToTrialMemberDate { get; set; }

        public virtual User? User { get; set; } = null!;
    }
}
