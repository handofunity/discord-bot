CREATE TABLE [config].[SpamProtectedChannel]
(
	[SpamProtectedChannelID] DECIMAL(20, 0) NOT NULL,
	[SoftCap] INT NOT NULL,
	[HardCap] INT NOT NULL,
    CONSTRAINT [PK_SpamProtectedChannel] PRIMARY KEY CLUSTERED ([SpamProtectedChannelID] ASC)
)
