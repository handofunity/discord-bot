﻿using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class GameRole
    {
        public short GameRoleID { get; set; }
        public decimal DiscordRoleID { get; set; }
        public string RoleName { get; set; }
        public short GameID { get; set; }
        public int ModifiedByUserID { get; set; }
        public DateTime ModifiedAtTimestamp { get; set; }

        public virtual Game Game { get; set; }
        public virtual User ModifiedByUser { get; set; }
    }
}