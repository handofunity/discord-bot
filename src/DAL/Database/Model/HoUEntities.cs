using Microsoft.EntityFrameworkCore;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class HoUEntities : DbContext
    {
        public virtual DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => new { e.DiscordUserID, e.UserID })
                    .HasName("IDX_User_DiscordUserID_Inc_UserID")
                    .IsUnique();

                entity.Property(m => m.UserID).ValueGeneratedOnAdd();
            });
        }
    }
}
