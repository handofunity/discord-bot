SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;
GO

IF EXISTS (select top 1 1 from [config].[GameRole])
    RAISERROR (N'Zeilen wurden erkannt. Das Schemaupdate wird beendet, da es möglicherweise zu einem Datenverlust kommt.', 16, 127) WITH NOWAIT
GO

PRINT N'[config].[UQ_GameRole_DiscordRoleID] wird gelöscht....';
GO

ALTER TABLE [config].[GameRole] DROP CONSTRAINT [UQ_GameRole_DiscordRoleID];
GO

PRINT N'[config].[GameRole] wird geändert....';
GO

ALTER TABLE [config].[GameRole] ALTER COLUMN [DiscordRoleID] DECIMAL (20) NOT NULL;
GO

PRINT N'[config].[UQ_GameRole_DiscordRoleID] wird erstellt....';
GO

ALTER TABLE [config].[GameRole]
    ADD CONSTRAINT [UQ_GameRole_DiscordRoleID] UNIQUE NONCLUSTERED ([DiscordRoleID] ASC);
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