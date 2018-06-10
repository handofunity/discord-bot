PRINT N'[config].[Game] wird erstellt....';


GO
CREATE TABLE [config].[Game] (
    [GameID]    SMALLINT      IDENTITY (1, 1) NOT NULL,
    [LongName]  VARCHAR (512) NOT NULL,
    [ShortName] VARCHAR (16)  NOT NULL,
    CONSTRAINT [PK_Game_GameID] PRIMARY KEY CLUSTERED ([GameID] ASC),
    CONSTRAINT [UQ_Game_LongName] UNIQUE NONCLUSTERED ([LongName] ASC),
    CONSTRAINT [UQ_Game_ShortName] UNIQUE NONCLUSTERED ([ShortName] ASC)
);


GO
PRINT N'[hou].[UserInfo] wird erstellt....';


GO
CREATE TABLE [hou].[UserInfo] (
    [UserID]   INT           NOT NULL,
    [LastSeen] DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_UserInfo_UserID] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
PRINT N'[hou].[FK_UserInfo_User] wird erstellt....';


GO
ALTER TABLE [hou].[UserInfo] WITH NOCHECK
    ADD CONSTRAINT [FK_UserInfo_User] FOREIGN KEY ([UserID]) REFERENCES [hou].[User] ([UserID]);


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
GRANT SELECT, INSERT, UPDATE, DELETE ON [hou].[UserInfo] TO [hou-guildbot];
GRANT SELECT, INSERT, DELETE ON [hou].[Vacation] TO [hou-guildbot];
GRANT SELECT, UPDATE ON [config].[Message] TO [hou-guildbot];
GRANT SELECT ON [config].[Game] TO [hou-guildbot];
GO
-- Scripts required for release start here
-- 1.0.0:
INSERT INTO config.Game
	(
		LongName,
		ShortName
	)
VALUES
	('Ashes of Creation', 'AoC'),
	('Bless Online', 'Bless');
GO

GO
PRINT N'Vorhandene Daten werden auf neu erstellte Einschränkungen hin überprüft.';
ALTER TABLE [hou].[UserInfo] WITH CHECK CHECK CONSTRAINT [FK_UserInfo_User];


GO
PRINT N'Update abgeschlossen.';


GO
