﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class HandOfUnityContext : DbContext
    {
        public HandOfUnityContext()
        {
        }

        public HandOfUnityContext(DbContextOptions<HandOfUnityContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Game> Game { get; set; }
        public virtual DbSet<GameRole> GameRole { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<Vacation> Vacation { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Game", "config");

                entity.HasIndex(e => e.LongName)
                    .HasName("UQ_Game_LongName")
                    .IsUnique();

                entity.HasIndex(e => e.ShortName)
                    .HasName("UQ_Game_ShortName")
                    .IsUnique();

                entity.Property(e => e.LongName)
                    .IsRequired()
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryGameDiscordRoleID).HasColumnType("decimal(20, 0)");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.HasOne(d => d.ModifiedByUser)
                    .WithMany(p => p.Game)
                    .HasForeignKey(d => d.ModifiedByUserID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<GameRole>(entity =>
            {
                entity.ToTable("GameRole", "config");

                entity.HasIndex(e => e.DiscordRoleID)
                    .HasName("UQ_GameRole_DiscordRoleID")
                    .IsUnique();

                entity.HasIndex(e => new { e.GameID, e.RoleName })
                    .HasName("UQ_GameRole_GameID_RoleName")
                    .IsUnique();

                entity.Property(e => e.DiscordRoleID).HasColumnType("decimal(20, 0)");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GameRole)
                    .HasForeignKey(d => d.GameID)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ModifiedByUser)
                    .WithMany(p => p.GameRole)
                    .HasForeignKey(d => d.ModifiedByUserID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("Message", "config");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(512);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User", "hou");

                entity.HasIndex(e => e.DiscordUserID)
                    .HasName("IDX_User_DiscordUserID_Inc_UserID")
                    .IsUnique();

                entity.Property(e => e.DiscordUserID).HasColumnType("decimal(20, 0)");
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasKey(e => e.UserID)
                    .HasName("PK_UserInfo_UserID");

                entity.ToTable("UserInfo", "hou");

                entity.Property(e => e.UserID).ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserInfo)
                    .HasForeignKey<UserInfo>(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserInfo_User");
            });

            modelBuilder.Entity<Vacation>(entity =>
            {
                entity.ToTable("Vacation", "hou");

                entity.Property(e => e.End).HasColumnType("date");

                entity.Property(e => e.Note)
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Start).HasColumnType("date");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Vacation)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}