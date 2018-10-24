SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;
GO

IF EXISTS (select top 1 1 from [config].[Game] AS g WHERE g.ModifiedByUserID IS NULL OR g.ModifiedAtTimestamp IS NULL)
    RAISERROR (N'Zeilen wurden erkannt. Das Schemaupdate wird beendet, da es möglicherweise zu einem Datenverlust kommt.', 16, 127) WITH NOWAIT
GO

PRINT N'[config].[FK_Game_User_ModifiedByUserID] wird gelöscht....';
GO

ALTER TABLE [config].[Game] DROP CONSTRAINT [FK_Game_User_ModifiedByUserID];
GO

PRINT N'[config].[Game] wird geändert....';
GO

ALTER TABLE [config].[Game] ALTER COLUMN [ModifiedAtTimestamp] DATETIME2 (7) NOT NULL;

ALTER TABLE [config].[Game] ALTER COLUMN [ModifiedByUserID] INT NOT NULL;
GO

PRINT N'[config].[FK_Game_User_ModifiedByUserID] wird erstellt....';
GO

ALTER TABLE [config].[Game] WITH NOCHECK
    ADD CONSTRAINT [FK_Game_User_ModifiedByUserID] FOREIGN KEY ([ModifiedByUserID]) REFERENCES [hou].[User] ([UserID]);
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

PRINT N'Vorhandene Daten werden auf neu erstellte Einschränkungen hin überprüft.';
GO

ALTER TABLE [config].[Game] WITH CHECK CHECK CONSTRAINT [FK_Game_User_ModifiedByUserID];
GO

PRINT N'Update abgeschlossen.';
GO