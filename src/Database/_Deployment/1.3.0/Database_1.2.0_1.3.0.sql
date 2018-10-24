SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;
GO

PRINT N'[config].[Game] wird geändert....';
GO

ALTER TABLE [config].[Game]
    ADD [ModifiedByUserID]    INT           NULL,
        [ModifiedAtTimestamp] DATETIME2 (7) NULL;
GO

PRINT N'[config].[GameRole] wird erstellt....';
GO

CREATE TABLE [config].[GameRole] (
    [GameRoleID]          SMALLINT      IDENTITY (1, 1) NOT NULL,
    [DiscordRoleID]       BIGINT        NOT NULL,
    [RoleName]            VARCHAR (512) NOT NULL,
    [GameID]              SMALLINT      NOT NULL,
    [ModifiedByUserID]    INT           NOT NULL,
    [ModifiedAtTimestamp] DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_GameRole_GameRoleID] PRIMARY KEY CLUSTERED ([GameRoleID] ASC),
    CONSTRAINT [UQ_GameRole_DiscordRoleID] UNIQUE NONCLUSTERED ([DiscordRoleID] ASC),
    CONSTRAINT [UQ_GameRole_GameID_RoleName] UNIQUE NONCLUSTERED ([GameID] ASC, [RoleName] ASC)
);
GO

PRINT N'[config].[FK_GameRole_User_ModifiedByUserID] wird erstellt....';
GO

ALTER TABLE [config].[GameRole] WITH NOCHECK
    ADD CONSTRAINT [FK_GameRole_User_ModifiedByUserID] FOREIGN KEY ([ModifiedByUserID]) REFERENCES [hou].[User] ([UserID]);
GO

PRINT N'[config].[FK_GameRole_Game_GameID] wird erstellt....';
GO

ALTER TABLE [config].[GameRole] WITH NOCHECK
    ADD CONSTRAINT [FK_GameRole_Game_GameID] FOREIGN KEY ([GameID]) REFERENCES [config].[Game] ([GameID]);
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


-- 1.3.0
DECLARE @userID INT;

SELECT @userID = u.UserID FROM [hou].[User] AS u
WHERE u.DiscordUserID = 179671767806115840;

UPDATE g
SET g.ModifiedByUserID = @userID,
	g.ModifiedAtTimestamp = SYSDATETIME()
FROM [config].[Game] AS g
GO

PRINT N'Vorhandene Daten werden auf neu erstellte Einschränkungen hin überprüft.';
GO

ALTER TABLE [config].[GameRole] WITH CHECK CHECK CONSTRAINT [FK_GameRole_User_ModifiedByUserID];

ALTER TABLE [config].[GameRole] WITH CHECK CHECK CONSTRAINT [FK_GameRole_Game_GameID];

ALTER TABLE [config].[Game] WITH CHECK CHECK CONSTRAINT [FK_Game_User_ModifiedByUserID];
GO

PRINT N'Update abgeschlossen.';
GO