CREATE TABLE [config].[GameRole]
(
	[GameRoleID] SMALLINT NOT NULL IDENTITY (1, 1),
	[DiscordRoleID] BIGINT NOT NULL,
	[RoleName] VARCHAR(512) NOT NULL,
	[GameID] SMALLINT NOT NULL,
	[ModifiedByUserID] INT NOT NULL,
	[ModifiedAtTimestamp] DATETIME2 NOT NULL,
	CONSTRAINT PK_GameRole_GameRoleID PRIMARY KEY (GameRoleID),
	CONSTRAINT UQ_GameRole_DiscordRoleID UNIQUE (DiscordRoleID),
	CONSTRAINT UQ_GameRole_GameID_RoleName UNIQUE (GameID, RoleName),
	CONSTRAINT FK_GameRole_Game_GameID FOREIGN KEY (GameID) REFERENCES [config].[Game] (GameID),
	CONSTRAINT FK_GameRole_User_ModifiedByUserID FOREIGN KEY (ModifiedByUserID) REFERENCES [hou].[User] (UserID)
)
