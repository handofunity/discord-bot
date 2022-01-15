CREATE TABLE [config].[Game] (
    [GameID]                                   SMALLINT NOT NULL IDENTITY(1, 1),
    [PrimaryGameDiscordRoleID]           DECIMAL(20, 0) NOT NULL,
    [ModifiedByUserID]                              INT NOT NULL,
    [ModifiedAtTimestamp]                     DATETIME2 NOT NULL,
    [IncludeInGuildMembersStatistic]                BIT NOT NULL,
    [IncludeInGamesMenu]                            BIT NOT NULL,
    [GameInterestRoleId]                 DECIMAL(20, 0) NULL,
    CONSTRAINT [PK_Game] PRIMARY KEY([GameID]),
    CONSTRAINT [UQ_Game_PrimaryGameDiscordRoleID] UNIQUE([PrimaryGameDiscordRoleID]),
    CONSTRAINT [FK_Game_User_ModifiedByUserID] FOREIGN KEY([ModifiedByUserID]) REFERENCES [hou].[User]([UserID]));
GO

CREATE UNIQUE INDEX [IDX_Game_NotNull_GameInterestRoleId] ON [config].[Game] ([GameInterestRoleId])
    WHERE GameInterestRoleId IS NOT NULL;
GO