IF NOT EXISTS(SELECT principal_id FROM [sys].[server_principals] WHERE name = 'HangFireUser') BEGIN
   -- check if the login exists, if not, it has to be setup manually
    RAISERROR ('Login ''HangFireUser'' is missing.', 16, 1)
	RETURN;
END
GO

IF EXISTS(SELECT principal_id FROM [sys].[database_principals] WHERE name = 'HangFireUser') BEGIN
	DROP USER [HangFireUser];
END
GO

CREATE USER [HangFireUser] FOR LOGIN [HangFireUser];
GO