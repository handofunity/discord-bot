CREATE TABLE [config].[ScheduledReminder]
(
	[ScheduledReminderID]	INT				NOT NULL IDENTITY(1, 1),
	[CronSchedule]			VARCHAR(64)		NOT NULL,
	[DiscordChannelID]      DECIMAL(20, 0)	NOT NULL,
	[Text]					VARCHAR(2048)	NOT NULL,
	CONSTRAINT [PK_ScheduledReminder] PRIMARY KEY CLUSTERED ([ScheduledReminderID] ASC)
)