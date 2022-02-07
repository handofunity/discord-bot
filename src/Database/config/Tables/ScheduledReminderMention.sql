CREATE TABLE [config].[ScheduledReminderMention]
(
	[ScheduledReminderMentionID]	INT				NOT NULL IDENTITY(1, 1),
	[ScheduledReminderID]			INT				NOT NULL,
	[DiscordUserID]					DECIMAL(20, 0)	NULL,
	[DiscordRoleID]					DECIMAL(20, 0)	NULL,
	CONSTRAINT [PK_ScheduledReminderMention] PRIMARY KEY CLUSTERED ([ScheduledReminderMentionID] ASC),
	CONSTRAINT [FK_ScheduledReminderMention_ScheduledReminder] FOREIGN KEY ([ScheduledReminderID]) REFERENCES [config].[ScheduledReminder] ([ScheduledReminderID])
)
GO

CREATE NONCLUSTERED INDEX [FK_ScheduledReminderMention_ScheduledReminder]
    ON [config].[ScheduledReminderMention]([ScheduledReminderID] ASC);
GO