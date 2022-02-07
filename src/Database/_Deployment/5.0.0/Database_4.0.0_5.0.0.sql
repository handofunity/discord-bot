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
			5,
			0,
			NULLIF(0, -1),
			NULLIF(-1, -1),
			SYSDATETIME()
		);

GO
PRINT N'Tabelle "[config].[PersonalReminder]" wird gelöscht...';


GO
DROP TABLE [config].[PersonalReminder];


GO
PRINT N'Tabelle "[config].[ScheduledReminder]" wird erstellt...';


GO
CREATE TABLE [config].[ScheduledReminder] (
    [ScheduledReminderID] INT            IDENTITY (1, 1) NOT NULL,
    [CronSchedule]        VARCHAR (64)   NOT NULL,
    [DiscordChannelID]    DECIMAL (20)   NOT NULL,
    [Text]                VARCHAR (2048) NOT NULL,
    CONSTRAINT [PK_ScheduledReminder] PRIMARY KEY CLUSTERED ([ScheduledReminderID] ASC)
);


GO
PRINT N'Tabelle "[config].[ScheduledReminderMention]" wird erstellt...';


GO
CREATE TABLE [config].[ScheduledReminderMention] (
    [ScheduledReminderMentionID] INT          IDENTITY (1, 1) NOT NULL,
    [ScheduledReminderID]        INT          NOT NULL,
    [DiscordUserID]              DECIMAL (20) NULL,
    [DiscordRoleID]              DECIMAL (20) NULL,
    CONSTRAINT [PK_ScheduledReminderMention] PRIMARY KEY CLUSTERED ([ScheduledReminderMentionID] ASC)
);


GO
PRINT N'Index "[config].[ScheduledReminderMention].[FK_ScheduledReminderMention_ScheduledReminder]" wird erstellt...';


GO
CREATE NONCLUSTERED INDEX [FK_ScheduledReminderMention_ScheduledReminder]
    ON [config].[ScheduledReminderMention]([ScheduledReminderID] ASC);


GO
PRINT N'Fremdschlüssel "[config].[FK_ScheduledReminderMention_ScheduledReminder]" wird erstellt...';


GO
ALTER TABLE [config].[ScheduledReminderMention] WITH NOCHECK
    ADD CONSTRAINT [FK_ScheduledReminderMention_ScheduledReminder] FOREIGN KEY ([ScheduledReminderID]) REFERENCES [config].[ScheduledReminder] ([ScheduledReminderID]);


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
GRANT SELECT, INSERT ON [config].[ScheduledReminder] TO [hou-guildbot];
GRANT SELECT, INSERT ON [config].[ScheduledReminderMention] TO [hou-guildbot];
GRANT SELECT ON [config].[Message] TO [hou-guildbot];
GRANT SELECT ON [config].[DiscordMapping] TO [hou-guildbot];
GRANT SELECT ON [config].[SpamProtectedChannel] TO [hou-guildbot];
GRANT SELECT ON [config].[DesiredTimeZone] TO [hou-guildbot];
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
ALTER TABLE [config].[ScheduledReminderMention] WITH CHECK CHECK CONSTRAINT [FK_ScheduledReminderMention_ScheduledReminder];


GO
PRINT N'Update abgeschlossen.';


GO
PRINT 'Tracking deployment execution time for current deployment'

GO
UPDATE [dv]
   SET [dv].[DeploymentEnd] = SYSDATETIME()
  FROM [dbo].[__DacpacVersion] AS [dv]
 WHERE [dv].[DacpacName] = N'Database'
   AND [dv].[Major] = 5
   AND [dv].[Minor] = 0
   AND ISNULL([dv].[Build], -1) = 0
   AND ISNULL([dv].[Revision], -1) = -1;

GO
