using System;
using System.Linq;
using System.Threading.Tasks;
using Cronos;
using Discord;
using Discord.Interactions;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.DAL.Discord.Modules;

public partial class ConfigModule
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Group("reminder", "Reminder configuration for the bot.")]
    public class ConfigReminderModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IConfigurationDatabaseAccess _configurationDatabaseAccess;
        private readonly IDynamicConfiguration _dynamicConfiguration;
        private readonly ILogger<ConfigReminderModule> _logger;

        public ConfigReminderModule(IConfigurationDatabaseAccess configurationDatabaseAccess,
                                    IDynamicConfiguration dynamicConfiguration,
                                    ILogger<ConfigReminderModule> logger)
        {
            _configurationDatabaseAccess = configurationDatabaseAccess;
            _dynamicConfiguration = dynamicConfiguration;
            _logger = logger;
        }

        [SlashCommand("add", "Adds a new scheduled reminder.", runMode: RunMode.Async)]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer | Role.Coordinator)]
        public async Task AddNewReminderAsync(
            [Summary(description: "When the reminder should trigger, see: https://crontab.guru/")] string cronSchedule,
            [Summary(description: "The channel to post the reminder in.")]
            ITextChannel textChannel,
            [Summary(description: "The text to post as reminder. DO NOT USE MENTIONS!")]
            string text)
        {
            await DeferAsync();

            DateTime? nextOccurrenceUtc;
            try
            {
                nextOccurrenceUtc = CronExpression.Parse(cronSchedule).GetNextOccurrence(DateTime.UtcNow);
            }
            catch (Exception e)
            {
                await
                    FollowupAsync($"Failed to get next occurrence. Cron expression might be invalid. **Error:** {e.GetBaseException().Message}");
                _logger.LogError(e, "Failed to get next occurrence. Cron expression might be invalid.");
                return;
            }

            var sri = new ScheduledReminderInfo(0,
                                                cronSchedule,
                                                (DiscordChannelId)textChannel.Id,
                                                Array.Empty<DiscordUserId>(),
                                                Array.Empty<DiscordRoleId>(),
                                                text);

            try
            {
                var scheduledReminderId = await _configurationDatabaseAccess.CreateScheduledReminderAsync(sri);
                await FollowupAsync("Successfully created a new reminder. "
                                  + $"**Scheduled Reminder Id:** `{scheduledReminderId}`. **Use this Id to add mentions.** "
                                  + $"Next occurrence of reminder (_guild time_): {(nextOccurrenceUtc?.ToString("dd.MM.yyyy HH:mm") ?? "<UNKNOWN>")}");
                await _dynamicConfiguration.LoadScheduledReminderInfosAsync();
            }
            catch (Exception e)
            {
                await FollowupAsync($"Failed to create reminder. **Error:** {e.GetBaseException().Message}");
                _logger.LogError(e, "Failed to create new reminder.");
            }
        }

        [SlashCommand("mention-user", "Adds a user mention to an existing scheduled reminder.")]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer | Role.Coordinator)]
        public async Task AddUserMentionAsync([Summary(description: "The Id of the scheduled reminder.")] int scheduledReminderId,
                                              [Summary(description: "The user to mention.")]
                                              IUser user)
        {
            var sri = await _configurationDatabaseAccess.GetScheduledReminderInfosAsync(scheduledReminderId);
            if (sri == null)
            {
                await RespondAsync($"Couldn't find scheduled reminder with Id {scheduledReminderId}.");
                return;
            }

            var userId = (DiscordUserId)user.Id;
            if (sri.RemindUsers.Contains(userId))
            {
                await RespondAsync($"User {user.Mention} is already in the mentioning list of this scheduled reminder.");
                return;
            }

            try
            {
                await _configurationDatabaseAccess.AddScheduledReminderMentionAsync(scheduledReminderId, userId);
                await RespondAsync($"Successfully added mention for {user.Mention}.");
            }
            catch (Exception e)
            {
                await RespondAsync($"Failed to add user mention. **Error:** {e.GetBaseException().Message}");
                _logger.LogError(e, "Failed to add user mention.");
            }
        }

        [SlashCommand("mention-role", "Adds a role mention to an existing scheduled reminder.")]
        [AllowedRoles(Role.Developer | Role.Leader | Role.Officer | Role.Coordinator)]
        public async Task AddRoleMentionAsync([Summary(description: "The Id of the scheduled reminder.")] int scheduledReminderId,
                                              [Summary(description: "The role to mention.")]
                                              IRole role)
        {
            var sri = await _configurationDatabaseAccess.GetScheduledReminderInfosAsync(scheduledReminderId);
            if (sri == null)
            {
                await RespondAsync($"Couldn't find scheduled reminder with Id {scheduledReminderId}.");
                return;
            }

            var roleId = (DiscordRoleId)role.Id;
            if (sri.RemindRoles.Contains(roleId))
            {
                await RespondAsync($"Role {role.Mention} is already in the mentioning list of this scheduled reminder.");
                return;
            }

            try
            {
                await _configurationDatabaseAccess.AddScheduledReminderMentionAsync(scheduledReminderId, roleId);
                await RespondAsync($"Successfully added mention for {role.Mention}.");
            }
            catch (Exception e)
            {
                await RespondAsync($"Failed to add role mention. **Error:** {e.GetBaseException().Message}");
                _logger.LogError(e, "Failed to add role mention.");
            }
        }
    }
}