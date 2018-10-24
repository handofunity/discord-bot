using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class Vacation
    {
        public int VacationID { get; set; }
        public int UserID { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Note { get; set; }

        public User User { get; set; }
    }
}
