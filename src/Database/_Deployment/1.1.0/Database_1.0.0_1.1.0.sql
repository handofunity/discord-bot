PRINT N'[config].[Message].[IDX_Message_Name_Inc_Content] wird gelöscht....';


GO
DROP INDEX [IDX_Message_Name_Inc_Content]
    ON [config].[Message];


GO
PRINT N'[config].[Message] wird geändert....';


GO
ALTER TABLE [config].[Message] ALTER COLUMN [Content] NVARCHAR (2000) NOT NULL;


GO
PRINT N'[config].[Message].[IDX_Message_Name_Inc_Content] wird erstellt....';


GO
CREATE UNIQUE NONCLUSTERED INDEX [IDX_Message_Name_Inc_Content]
    ON [config].[Message]([Name] ASC)
    INCLUDE([Content]);


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
GRANT SELECT, INSERT, UPDATE, DELETE ON [hou].[UserInfo] TO [hou-guildbot];
GRANT SELECT, INSERT, DELETE ON [hou].[Vacation] TO [hou-guildbot];
GRANT SELECT, UPDATE ON [config].[Message] TO [hou-guildbot];
GRANT SELECT ON [config].[Game] TO [hou-guildbot];
GO
-- 1.1.0:
INSERT INTO [config].[Message]
(
	[Name],
	[Description],
	[Content]
)
VALUES
(
	'WelcomeChannelMessage_01',
	'The first message the bot should create in the welcome channel.',
	'```cs
# Welcome to the Hand of Unity Discord.
# By default, only our public access channels are viewable and the guild specific channels are hidden, to gain ''Guest'' access please PM one of our Leadership or simply type in the chat.```'
),
(
	'WelcomeChannelMessage_02',
	'The second message the bot should create in the welcome channel.',
	'```Markdown
Roles and Leadership
====================
< Leader         > Our community has two co-leaders, Narys is responsible for ensuring our success in PVE content whereas Falmin is more focused upon securing our victory against fellow guilds and players
< Senior Officer > Together with the two co-leaders, these dedicated individuals make up the ruling council. They provide counsel to the leaders and supply our guild with whatever it needs
< Officer        > Individuals who have proven themselves loyal and hard-working. They are responsible for managing the community and fulfilling other roles such as recruitment, event management etc.
< Member         > The life blood of our community, our members are from a variety of different backgrounds and countries yet each individual strives to have fun and contribute to the greater community
< Recruit        > Applicants who have been accepted after submitting their application and being successful after interview are given recruit status. This is a trial period in order to assess your fit with us
< Guest          > Potential applicants who wish to learn more about the community and see if it is a fit for them can request this rank to see more of our Discord

Ruling Council
--------------
Narys             < Leader >
Falmin            < Leader >
Centego Rayven    < Senior Officer >
Herdo             < Senior Officer >
Palmer            < Senior Officer >
Raybird           < Senior Officer >

Officers
--------
Ariakira          < Officer >
SnowPaaw          < Officer >```'
),
(
	'WelcomeChannelMessage_03',
	'The third message the bot should create in the welcome channel.',
	'```Markdown
Our Rules
=========

1. Be respectful, if something you say is likely to offend an individual or group then quite simply don’t say it. Banter is fine, harassment is not.
2. Contribute to the community; whether this is in terms of finances, ideas, activities, chatting or helping our members. 
3. Members should be over the age of 18 given the mature nature of the community. 
4. Do not advertise other guilds or communities in Guild Chat/Discord/Guilded without permission from a Senior Officer or Leader.
5. English only in text channels and in large scale voice chat. 
6. Keep up to date on community announcements and participate.
7. If you join the official [Ashes of Creation discord server](https://discord.gg/Uf7emtS) when becoming a part of the community then it is mandatory to have the [HoU] guild tag in front of your name.
8. Use the Discord with respect, keep topics in the relevant channels and be considerate when in voice chat. Push to talk is strongly suggested. 
9. Keep discussion regarding religion out of the Discord. 
10. Sexual or NSFW images and links are not permitted.
 
Breaking one of these rules will result in a formal warning in the first instance. Should a second infraction occur then you will be removed from the community. If you wish to appeal either of these penalties, then please contact Narys or Falmin.```'
),
(
	'WelcomeChannelMessage_04',
	'The fourth message the bot should create in the welcome channel.',
	'```Markdown
Social
======

Server Invitation
-----------------

If you''d like to invite your friends to this Discord server, hand them out this link:
[Hand of Unity Discord server](https://discord.gg/KtjS7Fm)


Community Application
---------------------

If you''d like to join our community, head over to our application form:
[Hand of Unity application on guilded.gg](https://join.handofunity.eu)


Social Media
------------

Follow our social media channels:
1. [Hand of Unity YouTube channel](https://www.youtube.com/channel/UCk5Xu8X99vSJBNbeCOoZHNg)
2. [Hand of Unity Twitch channel](https://www.twitch.tv/handofunity)```'
);
GO
DELETE g
FROM config.Game g
WHERE g.ShortName = 'Bless'
GO
PRINT N'Update abgeschlossen.';


GO
