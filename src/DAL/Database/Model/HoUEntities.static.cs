namespace HoU.GuildBot.DAL.Database.Model
{
    using Microsoft.EntityFrameworkCore;

    public partial class HoUEntities
    {
        private readonly string _connectionString;

        public HoUEntities(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}