namespace HoU.GuildBot.DAL.Database.Model
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Message", Schema = "config")]
    public class Message
    {
        public int MessageID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}