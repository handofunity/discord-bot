CREATE TABLE [hou].[User] (
    [UserID]        INT				NOT NULL IDENTITY(1, 1),
    [DiscordUserID] DECIMAL(20, 0)	NOT NULL,
    CONSTRAINT [PK_User_UserID] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_User_DiscordUserID_Inc_UserID]
    ON [hou].[User]([DiscordUserID] ASC)
    INCLUDE([UserID]);