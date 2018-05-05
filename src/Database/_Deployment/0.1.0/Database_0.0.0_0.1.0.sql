PRINT N'[hou] wird erstellt....';


GO
CREATE SCHEMA [hou]
    AUTHORIZATION [dbo];


GO
PRINT N'[hou].[User] wird erstellt....';


GO
CREATE TABLE [hou].[User] (
    [UserID]        INT          IDENTITY (1, 1) NOT NULL,
    [DiscordUserID] DECIMAL (20) NOT NULL,
    CONSTRAINT [PK_User_UserID] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
PRINT N'[hou].[User].[IDX_User_DiscordUserID_Inc_UserID] wird erstellt....';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_User_DiscordUserID_Inc_UserID]
    ON [hou].[User]([DiscordUserID] ASC)
    INCLUDE([UserID]);


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
GO

GO
PRINT N'Update abgeschlossen.';


GO
