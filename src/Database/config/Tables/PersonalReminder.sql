CREATE TABLE [config].[PersonalReminder]
(
	[PersonalReminderID]	INT				NOT NULL IDENTITY(1, 1),
	[CronSchedule]			VARCHAR(64)		NOT NULL,
	[DiscordChannelID]      DECIMAL(20, 0)	NOT NULL,
	[UserToRemind]			DECIMAL(20, 0)	NOT NULL,
	[Text]					VARCHAR(1024)	NOT NULL,
	CONSTRAINT [PK_PersonalReminder] PRIMARY KEY CLUSTERED ([PersonalReminderID] ASC)
)
