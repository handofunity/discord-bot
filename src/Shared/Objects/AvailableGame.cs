namespace HoU.GuildBot.Shared.Objects
{
    using System.Collections.Generic;

    public class AvailableGame
    {
        public string LongName { get; set; }

        public string ShortName { get; set; }

        public ulong? PrimaryGameDiscordRoleID { get; set; }

        public List<AvailableGameRole> AvailableRoles { get; }

        public AvailableGame()
        {
            AvailableRoles = new List<AvailableGameRole>();
        }

        public AvailableGame Clone()
        {
            var c = new AvailableGame
            {
                LongName = LongName,
                ShortName = ShortName,
                PrimaryGameDiscordRoleID = PrimaryGameDiscordRoleID
            };

            foreach (var role in AvailableRoles)
            {
                c.AvailableRoles.Add(role.Clone());
            }

            return c;
        }
    }
}