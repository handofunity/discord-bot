CREATE TABLE [config].[Message]
(
	[MessageID]   INT            NOT NULL IDENTITY(1, 1),
	[Name]		  VARCHAR(128)   NOT NULL,
	[Description] NVARCHAR(512)  NOT NULL,
	[Content]     NVARCHAR(2000) NOT NULL,
	CONSTRAINT PK_Message_MessageID PRIMARY KEY (MessageID)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_Message_Name_Inc_Content]
    ON [config].[Message]([Name] ASC)
	INCLUDE([Content]);