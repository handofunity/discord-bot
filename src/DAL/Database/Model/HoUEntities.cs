using Microsoft.EntityFrameworkCore;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class HoUEntities : DbContext
    {
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Message> Message { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureSchemaConfig(modelBuilder);
            ConfigureSchemaHou(modelBuilder);
        }

        private static void ConfigureSchemaConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasIndex(e => new { e.Name, e.Content })
                      .HasName("IDX_Message_Name_Inc_Content")
                      .IsUnique();

                entity.Property(m => m.MessageID).ValueGeneratedOnAdd();
            });
        }

        private static void ConfigureSchemaHou(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => new {e.DiscordUserID, e.UserID})
                      .HasName("IDX_User_DiscordUserID_Inc_UserID")
                      .IsUnique();

                entity.Property(m => m.UserID).ValueGeneratedOnAdd();
            });
        }
    }
}
