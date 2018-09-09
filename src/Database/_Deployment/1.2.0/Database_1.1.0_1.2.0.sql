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
INSERT INTO [config].[Message]
(
	[Name],
	[Description],
	[Content]
)
VALUES
(
'AocRoleMenu',
'The message used as the role menu for AoC.',
'**ROLE MENU**
Add your reaction(s) to assign yourself the role(s).
Remove your reaction(s) to remove yourself from the role(s).

:musical_note: - Bard

:hospital: - Cleric

:crossed_swords: - Fighter

:fireworks: - Mage

:bow_and_arrow: - Ranger

:busts_in_silhouette: - Rogue

:hatching_chick: - Summoner

:shield: - Tank'
)
GO

GO
PRINT N'Update abgeschlossen.';


GO
