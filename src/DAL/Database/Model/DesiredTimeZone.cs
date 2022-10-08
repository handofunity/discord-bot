using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class DesiredTimeZone
    {
        public string DesiredTimeZoneKey { get; set; } = null!;
        public string InvariantDisplayName { get; set; } = null!;
    }
}
