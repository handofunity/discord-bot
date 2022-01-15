/*
Bereitstellungsskript für PRODUCTION

Dieser Code wurde von einem Tool generiert.
Änderungen an dieser Datei führen möglicherweise zu falschem Verhalten und gehen verloren, falls
der Code neu generiert wird.
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


GO
IF OBJECT_ID(N'[dbo].[__DacpacVersion]', N'U') IS NULL
BEGIN
	PRINT 'Creating table dbo.__DacpacVersion'
	CREATE TABLE [dbo].[__DacpacVersion]
	(
		DacpacVersionID	INT				NOT NULL	IDENTITY(1, 1),
		DacpacName		NVARCHAR(512)	NOT NULL,
		Major			INT				NOT NULL,
		Minor			INT				NOT NULL,
		Build			INT				NULL,
		Revision		INT				NULL,
		DeploymentStart	DATETIME2		NOT NULL,
		DeploymentEnd	DATETIME2		NULL,
		CONSTRAINT PK_DacpacVersion_DacpacVersionID PRIMARY KEY (DacpacVersionID)
	);
END

GO
PRINT 'Tracking version number for current deployment'

GO
INSERT INTO [dbo].[__DacpacVersion]
		   (DacpacName, Major, Minor, Build, Revision, DeploymentStart)
	VALUES
		(
			N'Database',
			4,
			0,
			NULLIF(0, -1),
			NULLIF(-1, -1),
			SYSDATETIME()
		);

GO
PRINT N'Fremdschlüssel "[config].[FK_Game_User_ModifiedByUserID]" wird gelöscht...';


GO
ALTER TABLE [config].[Game] DROP CONSTRAINT [FK_Game_User_ModifiedByUserID];


GO
PRINT N'Fremdschlüssel "[config].[FK_GameRole_Game_GameID]" wird gelöscht...';


GO
ALTER TABLE [config].[GameRole] DROP CONSTRAINT [FK_GameRole_Game_GameID];


GO
PRINT N'Unique-Einschränkung "[config].[UQ_Game_LongName]" wird gelöscht...';


GO
ALTER TABLE [config].[Game] DROP CONSTRAINT [UQ_Game_LongName];


GO
PRINT N'Unique-Einschränkung "[config].[UQ_Game_ShortName]" wird gelöscht...';


GO
ALTER TABLE [config].[Game] DROP CONSTRAINT [UQ_Game_ShortName];


GO
PRINT N'Unique-Einschränkung "[config].[UQ_GameRole_GameID_RoleName]" wird gelöscht...';


GO
ALTER TABLE [config].[GameRole] DROP CONSTRAINT [UQ_GameRole_GameID_RoleName];


GO
/*
Die Spalte "[config].[Game].[GameInterestEmojiName]" wird gelöscht, es könnte zu einem Datenverlust kommen.

Die Spalte "[config].[Game].[LongName]" wird gelöscht, es könnte zu einem Datenverlust kommen.

Die Spalte "[config].[Game].[ShortName]" wird gelöscht, es könnte zu einem Datenverlust kommen.

Die Spalte "PrimaryGameDiscordRoleID" der Tabelle "[config].[Game]" muss von NULL in NOT NULL geändert werden. Wenn die Tabelle Daten enthält, funktioniert das ALTER-Skript u. U. nicht. Um dieses Problem zu vermeiden, müssen Sie dieser Spalte für alle Zeilen Werte hinzufügen, sie so kennzeichnen, dass NULL-Werte zulässig sind, oder die Generierung von intelligenten Standardwerten als Bereitstellungsoption aktivieren.
*/
GO
PRINT N'Das erneute Erstellen der Tabelle "[config].[Game]" wird gestartet....';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [config].[tmp_ms_xx_Game] (
    [GameID]                         SMALLINT      IDENTITY (1, 1) NOT NULL,
    [PrimaryGameDiscordRoleID]       DECIMAL (20)  NOT NULL,
    [ModifiedByUserID]               INT           NOT NULL,
    [ModifiedAtTimestamp]            DATETIME2 (7) NOT NULL,
    [IncludeInGuildMembersStatistic] BIT           NOT NULL,
    [IncludeInGamesMenu]             BIT           NOT NULL,
    [GameInterestRoleId]             DECIMAL (20)  NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_Game1] PRIMARY KEY CLUSTERED ([GameID] ASC),
    CONSTRAINT [tmp_ms_xx_constraint_UQ_Game_PrimaryGameDiscordRoleID1] UNIQUE NONCLUSTERED ([PrimaryGameDiscordRoleID] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [config].[Game])
    BEGIN
        SET IDENTITY_INSERT [config].[tmp_ms_xx_Game] ON;
        INSERT INTO [config].[tmp_ms_xx_Game] ([GameID], [ModifiedByUserID], [ModifiedAtTimestamp], [PrimaryGameDiscordRoleID], [IncludeInGuildMembersStatistic], [IncludeInGamesMenu], [GameInterestRoleId])
        SELECT   [GameID],
                 [ModifiedByUserID],
                 [ModifiedAtTimestamp],
                 [PrimaryGameDiscordRoleID],
                 [IncludeInGuildMembersStatistic],
                 [IncludeInGamesMenu],
                 [GameInterestRoleId]
        FROM     [config].[Game]
        ORDER BY [GameID] ASC;
        SET IDENTITY_INSERT [config].[tmp_ms_xx_Game] OFF;
    END

DROP TABLE [config].[Game];

EXECUTE sp_rename N'[config].[tmp_ms_xx_Game]', N'Game';

EXECUTE sp_rename N'[config].[tmp_ms_xx_constraint_PK_Game1]', N'PK_Game', N'OBJECT';

EXECUTE sp_rename N'[config].[tmp_ms_xx_constraint_UQ_Game_PrimaryGameDiscordRoleID1]', N'UQ_Game_PrimaryGameDiscordRoleID', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Index "[config].[Game].[IDX_Game_NotNull_GameInterestRoleId]" wird erstellt...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_Game_NotNull_GameInterestRoleId]
    ON [config].[Game]([GameInterestRoleId] ASC) WHERE GameInterestRoleId IS NOT NULL;


GO
PRINT N'Tabelle "[config].[GameRole]" wird geändert...';


GO
ALTER TABLE [config].[GameRole] DROP COLUMN [RoleName];


GO
PRINT N'Fremdschlüssel "[config].[FK_Game_User_ModifiedByUserID]" wird erstellt...';


GO
ALTER TABLE [config].[Game] WITH NOCHECK
    ADD CONSTRAINT [FK_Game_User_ModifiedByUserID] FOREIGN KEY ([ModifiedByUserID]) REFERENCES [hou].[User] ([UserID]);


GO
PRINT N'Fremdschlüssel "[config].[FK_GameRole_Game_GameID]" wird erstellt...';


GO
ALTER TABLE [config].[GameRole] WITH NOCHECK
    ADD CONSTRAINT [FK_GameRole_Game_GameID] FOREIGN KEY ([GameID]) REFERENCES [config].[Game] ([GameID]);


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
GO

IF EXISTS(SELECT principal_id FROM [sys].[database_principals] WHERE name = 'hou-guildbot') BEGIN
	DROP USER [hou-guildbot];
END
GO

CREATE USER [hou-guildbot] FOR LOGIN [hou-guildbot];
GO
GO
GRANT SELECT, INSERT ON [hou].[User] TO [hou-guildbot];
GRANT SELECT, INSERT, UPDATE, DELETE ON [hou].[UserInfo] TO [hou-guildbot];
GRANT SELECT, INSERT, DELETE ON [hou].[Vacation] TO [hou-guildbot];
GRANT SELECT, INSERT, UPDATE, DELETE ON [config].[Game] TO [hou-guildbot];
GRANT SELECT, INSERT, UPDATE, DELETE ON [config].[GameRole] TO [hou-guildbot];
GRANT SELECT ON [config].[Message] TO [hou-guildbot];
GRANT SELECT ON [config].[DiscordMapping] TO [hou-guildbot];
GRANT SELECT ON [config].[SpamProtectedChannel] TO [hou-guildbot];
GRANT SELECT ON [config].[DesiredTimeZone] TO [hou-guildbot];
GRANT SELECT ON [config].[PersonalReminder] TO [hou-guildbot];
GRANT SELECT ON [config].[UnitsEndpoint] TO [hou-guildbot];
GO
IF NOT EXISTS
(
    SELECT
        [principal_id]
    FROM  [sys].[server_principals]
    WHERE [name] COLLATE Latin1_General_CI_AS = 'HangFireUser' COLLATE Latin1_General_CI_AS
)
    BEGIN
        -- check if the login exists, if not, it has to be setup manually
        RAISERROR('Login ''HangFireUser'' is missing.', 16, 1);
        RETURN;
END;
GO

IF NOT EXISTS
(
    SELECT
        [principal_id]
    FROM  [sys].[database_principals]
    WHERE [name] COLLATE Latin1_General_CI_AS = 'HangFireUser' COLLATE Latin1_General_CI_AS
)
    BEGIN
        CREATE USER [HangFireUser] FOR LOGIN [HangFireUser];
    END;
GO
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = 'HangFire') EXEC ('CREATE SCHEMA [HangFire]')
GO

ALTER AUTHORIZATION ON SCHEMA::[HangFire] TO [HangFireUser]
GO

GRANT CREATE TABLE TO [HangFireUser]
GO
GO

----------------------------------------------------
----------------------------------------------------
----- VERSION SPECIFIC POST DEPLOYMENT SCRIPTS -----
----------------------------------------------------
----------------------------------------------------

/*
-- 0.2.0:
:r .\Scripts\AddMessage_FirstServerJoinWelcome.sql
GO
-- 1.0.0:
:r .\Scripts\AddInitialGames.sql
GO
-- 1.1.0:
:r .\Scripts\AddMessages_WelcomeChannelMessages.sql
GO
:r .\Scripts\RemoveGame_Bless.sql
GO
-- 1.2.0:
:r .\Scripts\AddMessage_AocRoleMenu.sql
GO
-- 1.3.0
:r .\Scripts\MigrateGameTable_1.2.0_1.3.0.sql
GO
-- 1.5.0:
:r .\Scripts\FillGameTable_IncludeInGuildMembersStatisticColumn.sql
GO
-- 2.2.1
:r .\Scripts\RemoveMessage_FirstServerJoinWelcome.sql
GO
-- x.y.z:
*/
GO

GO
PRINT N'Vorhandene Daten werden auf neu erstellte Einschränkungen hin überprüft.';


GO
ALTER TABLE [config].[Game] WITH CHECK CHECK CONSTRAINT [FK_Game_User_ModifiedByUserID];

ALTER TABLE [config].[GameRole] WITH CHECK CHECK CONSTRAINT [FK_GameRole_Game_GameID];


GO
PRINT N'Update abgeschlossen.';


GO
PRINT 'Tracking deployment execution time for current deployment'

GO
UPDATE [dv]
   SET [dv].[DeploymentEnd] = SYSDATETIME()
  FROM [dbo].[__DacpacVersion] AS [dv]
 WHERE [dv].[DacpacName] = N'Database'
   AND [dv].[Major] = 4
   AND [dv].[Minor] = 0
   AND ISNULL([dv].[Build], -1) = 0
   AND ISNULL([dv].[Revision], -1) = -1;

GO
