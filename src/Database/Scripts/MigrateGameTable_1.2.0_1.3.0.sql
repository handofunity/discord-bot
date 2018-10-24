DECLARE @userID INT;

SELECT @userID = u.UserID FROM [hou].[User] AS u
WHERE u.DiscordUserID = 179671767806115840;

UPDATE g
SET g.ModifiedByUserID = @userID,
	g.ModifiedAtTimestamp = SYSDATETIME()
FROM [config].[Game] AS g