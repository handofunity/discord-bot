using System;
using System.Collections.Generic;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class KeycloakEndpoint
    {
        public int KeycloakEndpointId { get; set; }
        public string BaseUrl { get; set; } = null!;
        public string AccessTokenUrl { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Realm { get; set; } = null!;
    }
}
