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