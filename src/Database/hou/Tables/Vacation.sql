CREATE TABLE [hou].[Vacation]
(
	[VacationID]	INT				NOT NULL IDENTITY(1, 1),
	[UserID]		INT				NOT NULL,
	[Start]			DATE			NOT NULL,
	[End]			DATE			NOT NULL,
	[Note]			VARCHAR(1024)	NULL,
	CONSTRAINT PK_Vacation_VacationID PRIMARY KEY CLUSTERED ([VacationID] ASC),
	CONSTRAINT FK_Vacation_User_UserID FOREIGN KEY ([UserID]) REFERENCES [hou].[User] ([UserID])
)
