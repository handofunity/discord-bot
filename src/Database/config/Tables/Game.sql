﻿CREATE TABLE [config].[Game]
(
	[GameID] SMALLINT NOT NULL IDENTITY (1, 1),
	[LongName] VARCHAR(512) NOT NULL,
	[ShortName] VARCHAR(16) NOT NULL,
	CONSTRAINT PK_Game_GameID PRIMARY KEY (GameID),
	CONSTRAINT UQ_Game_LongName UNIQUE (LongName),
	CONSTRAINT UQ_Game_ShortName UNIQUE (ShortName)
)