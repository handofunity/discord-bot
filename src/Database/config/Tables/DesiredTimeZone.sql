CREATE TABLE [config].[DesiredTimeZone]
(
	[DesiredTimeZoneKey] VARCHAR(128) NOT NULL,
	[InvariantDisplayName] VARCHAR(1024) NOT NULL,
	CONSTRAINT [PK_DesiredTimeZone] PRIMARY KEY CLUSTERED ([DesiredTimeZoneKey] ASC)
)
