PRINT N'[config] wird erstellt....';


GO
CREATE SCHEMA [config]
    AUTHORIZATION [dbo];


GO
PRINT N'[config].[Message] wird erstellt....';


GO
CREATE TABLE [config].[Message] (
    [MessageID]   INT             IDENTITY (1, 1) NOT NULL,
    [Name]        [sysname]       NOT NULL,
    [Description] NVARCHAR (512)  NOT NULL,
    [Content]     NVARCHAR (4000) NOT NULL
);


GO
PRINT N'[config].[Message].[IDX_Message_Name_Inc_Content] wird erstellt....';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_Message_Name_Inc_Content]
    ON [config].[Message]([Name] ASC)
    INCLUDE([Content]);


GO
IF NOT EXISTS(SELECT principal_id FROM [sys].[server_principals] WHERE name = 'hou-guildbot') BEGIN
   -- check if the login exists, if not, it has to be setup manually
    RAISERROR ('Login ''hou-guildbot'' is missing.', 16, 1)
	RETURN;
END

IF NOT EXISTS(SELECT principal_id FROM [sys].[database_principals] WHERE name = 'hou-guildbot') BEGIN
    CREATE USER [hou-guildbot] FOR LOGIN [hou-guildbot]
END
GO
GRANT SELECT, INSERT ON [hou].[User] TO [hou-guildbot];
GRANT SELECT, UPDATE ON [config].[Message] TO [hou-guildbot];
GO
INSERT INTO [config].[Message]
(
	[Name],
	[Description],
	[Content]
)
VALUES
(
	'FirstServerJoinWelcome',
	'The welcome message sent to users who join the server for the first time ever.',
	'Welcome to the Hand of Unity Discord. ' + 
		'As default, vision/use of our text and voice channels is granted to people with guest permissions only (hence why the Discord seems empty). ' +
		'If you would like to access our actual guild areas to participate then please contact Narys or type in the public lobby.'
);
GO

GO
PRINT N'Update abgeschlossen.';


GO
