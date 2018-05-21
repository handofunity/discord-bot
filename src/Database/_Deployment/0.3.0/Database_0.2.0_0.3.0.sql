PRINT N'[hou].[Vacation] wird erstellt....';


GO
CREATE TABLE [hou].[Vacation] (
    [VacationID] INT            IDENTITY (1, 1) NOT NULL,
    [UserID]     INT            NOT NULL,
    [Start]      DATE           NOT NULL,
    [End]        DATE           NOT NULL,
    [Note]       VARCHAR (1024) NULL,
    CONSTRAINT [PK_Vacation_VacationID] PRIMARY KEY CLUSTERED ([VacationID] ASC)
);


GO
PRINT N'[hou].[FK_Vacation_User_UserID] wird erstellt....';


GO
ALTER TABLE [hou].[Vacation] WITH NOCHECK
    ADD CONSTRAINT [FK_Vacation_User_UserID] FOREIGN KEY ([UserID]) REFERENCES [hou].[User] ([UserID]);


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
GRANT SELECT, INSERT, DELETE ON [hou].[Vacation] TO [hou-guildbot];
GRANT SELECT, UPDATE ON [config].[Message] TO [hou-guildbot];
GO

GO
PRINT N'Vorhandene Daten werden auf neu erstellte Einschränkungen hin überprüft.';


GO
ALTER TABLE [hou].[Vacation] WITH CHECK CHECK CONSTRAINT [FK_Vacation_User_UserID];


GO
PRINT N'Update abgeschlossen.';


GO
