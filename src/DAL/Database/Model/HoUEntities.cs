using Microsoft.EntityFrameworkCore;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class HoUEntities : DbContext
    {
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<Vacation> Vacation { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<Game> Game { get; set; }

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
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasIndex(m => m.LongName)
                      .HasName("UQ_Game_LongName")
                      .IsUnique();

                entity.HasIndex(m => m.ShortName)
                      .HasName("UQ_Game_ShortName")
                      .IsUnique();

                entity.Property(m => m.GameID).ValueGeneratedOnAdd();
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
            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasKey(m => m.UserID);
                entity.HasOne(m => m.User).WithOne(m => m.UserInfo);
            });
            modelBuilder.Entity<Vacation>(entity =>
            {
                entity.HasOne(m => m.User)
                      .WithMany(m => m.Vacations);

                entity.Property(m => m.VacationID).ValueGeneratedOnAdd();
            });
        }
    }
}
