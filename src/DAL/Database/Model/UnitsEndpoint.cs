using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class UnitsEndpoint
    {
        public int UnitsEndpointId { get; set; }
        public string BaseAddress { get; set; } = null!;
        public bool ConnectToRestApi { get; set; }
        public bool ConnectToNotificationsHub { get; set; }
        public int KeycloakEndpointId { get; set; }
        public decimal? NewEventPingDiscordRoleId { get; set; }

        public virtual KeycloakEndpoint? KeycloakEndpoint { get; set; } = null!;
    }
}
