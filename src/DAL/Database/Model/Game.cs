namespace HoU.GuildBot.DAL.Database.Model
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Game", Schema = "config")]
    public class Game
    {
        public short GameID { get; set; }

        public string LongName { get; set; }

        public string ShortName { get; set; }
    }
}