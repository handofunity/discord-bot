using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HoU.GuildBot.DAL.Database.Model
{
    public partial class HandOfUnityContext : DbContext
    {
        public virtual DbSet<DesiredTimeZone> DesiredTimeZone { get; set; } = null!;
        public virtual DbSet<DiscordMapping> DiscordMapping { get; set; } = null!;
        public virtual DbSet<Game> Game { get; set; } = null!;
        public virtual DbSet<GameRole> GameRole { get; set; } = null!;
        public virtual DbSet<KeycloakEndpoint> KeycloakEndpoint { get; set; } = null!;
        public virtual DbSet<Message> Message { get; set; } = null!;
        public virtual DbSet<ScheduledReminder> ScheduledReminder { get; set; } = null!;
        public virtual DbSet<ScheduledReminderMention> ScheduledReminderMention { get; set; } = null!;
        public virtual DbSet<SpamProtectedChannel> SpamProtectedChannel { get; set; } = null!;
        public virtual DbSet<UnitsEndpoint> UnitsEndpoint { get; set; } = null!;
        public virtual DbSet<User> User { get; set; } = null!;
        public virtual DbSet<UserBirthday> UserBirthday { get; set; } = null!;
        public virtual DbSet<UserInfo> UserInfo { get; set; } = null!;
        public virtual DbSet<Vacation> Vacation { get; set; } = null!;

        public HandOfUnityContext(DbContextOptions<HandOfUnityContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DesiredTimeZone>(entity =>
            {
                entity.HasKey(e => e.DesiredTimeZoneKey)
                    .HasName("desired_time_zone_pkey");

                entity.ToTable("desired_time_zone", "config");

                entity.Property(e => e.DesiredTimeZoneKey)
                    .HasMaxLength(128)
                    .HasColumnName("desired_time_zone_key");

                entity.Property(e => e.InvariantDisplayName)
                    .HasMaxLength(1024)
                    .HasColumnName("invariant_display_name");
            });

            modelBuilder.Entity<DiscordMapping>(entity =>
            {
                entity.HasKey(e => e.DiscordMappingKey)
                    .HasName("discord_mapping_pkey");

                entity.ToTable("discord_mapping", "config");

                entity.Property(e => e.DiscordMappingKey)
                    .HasMaxLength(64)
                    .HasColumnName("discord_mapping_key");

                entity.Property(e => e.DiscordId)
                    .HasPrecision(20)
                    .HasColumnName("discord_id");
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("game", "config");

                entity.HasIndex(e => e.GameInterestRoleId, "idx_game_not_null_game_interest_role_id")
                    .IsUnique()
                    .HasFilter("(game_interest_role_id IS NOT NULL)");

                entity.HasIndex(e => e.PrimaryGameDiscordRoleId, "uq_game_primary_game_discord_role_id")
                    .IsUnique();

                entity.Property(e => e.GameId)
                    .HasColumnName("game_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.GameInterestRoleId)
                    .HasPrecision(20)
                    .HasColumnName("game_interest_role_id");

                entity.Property(e => e.IncludeInGamesMenu).HasColumnName("include_in_games_menu");

                entity.Property(e => e.IncludeInGuildMembersStatistic).HasColumnName("include_in_guild_members_statistic");

                entity.Property(e => e.ModifiedAtTimestamp).HasColumnName("modified_at_timestamp");

                entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");

                entity.Property(e => e.PrimaryGameDiscordRoleId)
                    .HasPrecision(20)
                    .HasColumnName("primary_game_discord_role_id");

                entity.HasOne(d => d.ModifiedByUser)
                    .WithMany(p => p.Game)
                    .HasForeignKey(d => d.ModifiedByUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_game_user");
            });

            modelBuilder.Entity<GameRole>(entity =>
            {
                entity.ToTable("game_role", "config");

                entity.HasIndex(e => e.DiscordRoleId, "uq_game_role_discord_role_id")
                    .IsUnique();

                entity.Property(e => e.GameRoleId)
                    .HasColumnName("game_role_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DiscordRoleId)
                    .HasPrecision(20)
                    .HasColumnName("discord_role_id");

                entity.Property(e => e.GameId).HasColumnName("game_id");

                entity.Property(e => e.ModifiedAtTimestamp).HasColumnName("modified_at_timestamp");

                entity.Property(e => e.ModifiedByUserId).HasColumnName("modified_by_user_id");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.GameRole)
                    .HasForeignKey(d => d.GameId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_game_role_game_game_id");

                entity.HasOne(d => d.ModifiedByUser)
                    .WithMany(p => p.GameRole)
                    .HasForeignKey(d => d.ModifiedByUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_game_role_user_modified_by_user_id");
            });

            modelBuilder.Entity<KeycloakEndpoint>(entity =>
            {
                entity.ToTable("keycloak_endpoint", "config");

                entity.HasIndex(e => new { e.BaseUrl, e.Realm }, "uq_base_url_realm")
                    .IsUnique();

                entity.Property(e => e.KeycloakEndpointId)
                    .HasColumnName("keycloak_endpoint_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.AccessTokenUrl)
                    .HasMaxLength(256)
                    .HasColumnName("access_token_url");

                entity.Property(e => e.BaseUrl)
                    .HasMaxLength(256)
                    .HasColumnName("base_url");

                entity.Property(e => e.ClientId)
                    .HasMaxLength(128)
                    .HasColumnName("client_id");

                entity.Property(e => e.ClientSecret)
                    .HasMaxLength(128)
                    .HasColumnName("client_secret");

                entity.Property(e => e.Realm)
                    .HasMaxLength(128)
                    .HasColumnName("realm");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("message", "config");

                entity.HasIndex(e => e.Name, "idx_message_name_inc_content")
                    .IsUnique();

                entity.Property(e => e.MessageId)
                    .HasColumnName("message_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Content)
                    .HasMaxLength(2000)
                    .HasColumnName("content");

                entity.Property(e => e.Description)
                    .HasMaxLength(512)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .HasMaxLength(128)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<ScheduledReminder>(entity =>
            {
                entity.ToTable("scheduled_reminder", "config");

                entity.Property(e => e.ScheduledReminderId)
                    .HasColumnName("scheduled_reminder_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CronSchedule)
                    .HasMaxLength(64)
                    .HasColumnName("cron_schedule");

                entity.Property(e => e.DiscordChannelId)
                    .HasPrecision(20)
                    .HasColumnName("discord_channel_id");

                entity.Property(e => e.Text)
                    .HasMaxLength(2048)
                    .HasColumnName("text");
            });

            modelBuilder.Entity<ScheduledReminderMention>(entity =>
            {
                entity.ToTable("scheduled_reminder_mention", "config");

                entity.Property(e => e.ScheduledReminderMentionId)
                    .HasColumnName("scheduled_reminder_mention_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DiscordRoleId)
                    .HasPrecision(20)
                    .HasColumnName("discord_role_id");

                entity.Property(e => e.DiscordUserId)
                    .HasPrecision(20)
                    .HasColumnName("discord_user_id");

                entity.Property(e => e.ScheduledReminderId).HasColumnName("scheduled_reminder_id");

                entity.HasOne(d => d.ScheduledReminder)
                    .WithMany(p => p.ScheduledReminderMention)
                    .HasForeignKey(d => d.ScheduledReminderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_scheduled_reminder_mention_scheduled_reminder");
            });

            modelBuilder.Entity<SpamProtectedChannel>(entity =>
            {
                entity.ToTable("spam_protected_channel", "config");

                entity.Property(e => e.SpamProtectedChannelId)
                    .HasPrecision(20)
                    .HasColumnName("spam_protected_channel_id");

                entity.Property(e => e.HardCap).HasColumnName("hard_cap");

                entity.Property(e => e.SoftCap).HasColumnName("soft_cap");
            });

            modelBuilder.Entity<UnitsEndpoint>(entity =>
            {
                entity.ToTable("units_endpoint", "config");

                entity.HasIndex(e => e.BaseAddress, "uq_base_address")
                    .IsUnique();

                entity.HasIndex(e => e.Chapter, "uq_chapter")
                    .IsUnique();

                entity.Property(e => e.UnitsEndpointId)
                    .HasColumnName("units_endpoint_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.BaseAddress)
                    .HasMaxLength(256)
                    .HasColumnName("base_address");

                entity.Property(e => e.Chapter)
                    .HasMaxLength(32)
                    .HasColumnName("chapter");

                entity.Property(e => e.ConnectToNotificationsHub).HasColumnName("connect_to_notifications_hub");

                entity.Property(e => e.ConnectToRestApi).HasColumnName("connect_to_rest_api");

                entity.Property(e => e.KeycloakEndpointId).HasColumnName("keycloak_endpoint_id");

                entity.Property(e => e.NewEventPingDiscordRoleId)
                    .HasPrecision(20)
                    .HasColumnName("new_event_ping_discord_role_id");

                entity.Property(e => e.NewRequisitionOrderPingDiscordRoleId)
                    .HasPrecision(20)
                    .HasColumnName("new_requisition_order_ping_discord_role_id");

                entity.HasOne(d => d.KeycloakEndpoint)
                    .WithMany(p => p.UnitsEndpoint)
                    .HasForeignKey(d => d.KeycloakEndpointId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("units_endpoint_keycloak_endpoint_id_fkey");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user", "hou");

                entity.HasIndex(e => e.DiscordUserId, "idx_user_discord_user_id")
                    .IsUnique();

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.DiscordUserId)
                    .HasPrecision(20)
                    .HasColumnName("discord_user_id");
            });

            modelBuilder.Entity<UserBirthday>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("user_birthday_pkey");

                entity.ToTable("user_birthday", "hou");

                entity.Property(e => e.UserId)
                    .ValueGeneratedNever()
                    .HasColumnName("user_id");

                entity.Property(e => e.Day).HasColumnName("day");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserBirthday)
                    .HasForeignKey<UserBirthday>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_birthday_user");
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("user_info_pkey");

                entity.ToTable("user_info", "hou");

                entity.Property(e => e.UserId)
                    .ValueGeneratedNever()
                    .HasColumnName("user_id");

                entity.Property(e => e.CurrentRoles)
                    .HasMaxLength(32768)
                    .HasColumnName("current_roles");

                entity.Property(e => e.JoinedDate).HasColumnName("joined_date");

                entity.Property(e => e.LastSeen).HasColumnName("last_seen");

                entity.Property(e => e.PromotedToTrialMemberDate).HasColumnName("promoted_to_trial_member_date");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserInfo)
                    .HasForeignKey<UserInfo>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_info_user");
            });

            modelBuilder.Entity<Vacation>(entity =>
            {
                entity.ToTable("vacation", "hou");

                entity.Property(e => e.VacationId)
                    .HasColumnName("vacation_id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.EndDate).HasColumnName("end_date");

                entity.Property(e => e.Note)
                    .HasMaxLength(1024)
                    .HasColumnName("note");

                entity.Property(e => e.StartDate).HasColumnName("start_date");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Vacation)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_vacation_user");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
