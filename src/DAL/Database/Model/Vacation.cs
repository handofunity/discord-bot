namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class Vacation
    {
        public int VacationId { get; set; }
        public int UserId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Note { get; set; }

        public virtual User? User { get; set; } = null!;
    }
}
