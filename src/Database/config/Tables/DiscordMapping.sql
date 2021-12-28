CREATE TABLE [config].[DiscordMapping]
(
	[DiscordMappingKey] VARCHAR(64)	NOT NULL,
    [DiscordID] DECIMAL(20, 0)		NOT NULL,
    CONSTRAINT [PK_DiscordMapping] PRIMARY KEY CLUSTERED ([DiscordMappingKey] ASC)
)