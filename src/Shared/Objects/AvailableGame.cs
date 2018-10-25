namespace HoU.GuildBot.Shared.Objects
{
    using System.Collections.Generic;

    public class AvailableGame
    {
        public string LongName { get; set; }

        public string ShortName { get; set; }

        public List<AvailableGameRole> AvailableRoles { get; }

        public AvailableGame()
        {
            AvailableRoles = new List<AvailableGameRole>();
        }
    }
}