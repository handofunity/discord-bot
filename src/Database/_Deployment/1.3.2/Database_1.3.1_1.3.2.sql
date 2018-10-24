SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;
GO

PRINT N'[config].[PK_Message_MessageID] wird erstellt....';
GO

ALTER TABLE [config].[Message]
    ADD CONSTRAINT [PK_Message_MessageID] PRIMARY KEY CLUSTERED ([MessageID] ASC);
GO

-------------------------------------------
-------------------------------------------
----- GENERAL POST DEPLOYMENT SCRIPTS -----
-------------------------------------------
-------------------------------------------

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
GRANT SELECT, INSERT, UPDATE, DELETE ON [hou].[UserInfo] TO [hou-guildbot];
GRANT SELECT, INSERT, DELETE ON [hou].[Vacation] TO [hou-guildbot];
GRANT SELECT, UPDATE ON [config].[Message] TO [hou-guildbot];
GRANT SELECT, INSERT, UPDATE, DELETE ON [config].[Game] TO [hou-guildbot];
GRANT SELECT, INSERT, UPDATE, DELETE ON [config].[GameRole] TO [hou-guildbot]
GO

----------------------------------------------------
----------------------------------------------------
----- VERSION SPECIFIC POST DEPLOYMENT SCRIPTS -----
----------------------------------------------------
----------------------------------------------------
GO

PRINT N'Update abgeschlossen.';
GO