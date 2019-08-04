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
			2,
			1,
			NULLIF(0, -1),
			NULLIF(-1, -1),
			SYSDATETIME()
		);

GO
PRINT N'[config].[Game] wird geändert....';


GO
ALTER TABLE [config].[Game]
    ADD [IncludeInGamesMenu] BIT CONSTRAINT [DF_Game_IncludeInGamesMenu] DEFAULT 1 NOT NULL;


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
GRANT SELECT, UPDATE ON [config].[Message] TO [hou-guildbot];
GRANT SELECT, INSERT, UPDATE, DELETE ON [config].[Game] TO [hou-guildbot];
GRANT SELECT, INSERT, UPDATE, DELETE ON [config].[GameRole] TO [hou-guildbot]
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
-- x.y.z:
*/
GO

GO
PRINT N'Update abgeschlossen.';


GO
PRINT 'Tracking deployment execution time for current deployment'

GO
UPDATE [dv]
   SET [dv].[DeploymentEnd] = SYSDATETIME()
  FROM [dbo].[__DacpacVersion] AS [dv]
 WHERE [dv].[DacpacName] = N'Database'
   AND [dv].[Major] = 2
   AND [dv].[Minor] = 1
   AND ISNULL([dv].[Build], -1) = 0
   AND ISNULL([dv].[Revision], -1) = -1;

GO
