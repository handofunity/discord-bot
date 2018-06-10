namespace HoU.GuildBot.Shared.Objects
{
    using System.Collections.Generic;

    public class AvailableGame
    {
        public string LongName { get; set; }

        public string ShortName { get; set; }

        public List<string> ClassNames { get; }

        public AvailableGame()
        {
            ClassNames = new List<string>();
        }
    }
}