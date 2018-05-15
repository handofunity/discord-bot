CREATE TABLE [config].[Message]
(
	[MessageID]   INT            NOT NULL IDENTITY(1, 1),
	[Name]        SYSNAME        NOT NULL,
	[Description] NVARCHAR(512)  NOT NULL,
	[Content]     NVARCHAR(4000) NOT NULL
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_Message_Name_Inc_Content]
    ON [config].[Message]([Name] ASC)
	INCLUDE([Content]);