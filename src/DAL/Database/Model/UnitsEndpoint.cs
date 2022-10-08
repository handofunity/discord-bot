using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class UnitsEndpoint
    {
        public int UnitsEndpointId { get; set; }
        public string BaseAddress { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public bool ConnectToRestApi { get; set; }
        public bool ConnectToNotificationsHub { get; set; }
    }
}
