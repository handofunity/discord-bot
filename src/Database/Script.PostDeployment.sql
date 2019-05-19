-------------------------------------------
-------------------------------------------
----- GENERAL POST DEPLOYMENT SCRIPTS -----
-------------------------------------------
-------------------------------------------

:r .\Scripts\CreateUser.sql
GO
:r .\Scripts\GrantPermissions.sql
GO
----------------------------------------------------
----------------------------------------------------
----- VERSION SPECIFIC POST DEPLOYMENT SCRIPTS -----
----------------------------------------------------
----------------------------------------------------

/*
-- 0.2.0:
:r .\Scripts\AddMessage_FirstServerJoinWelcome.sql
GO
-- 1.0.0:
:r .\Scripts\AddInitialGames.sql
GO
-- 1.1.0:
:r .\Scripts\AddMessages_WelcomeChannelMessages.sql
GO
:r .\Scripts\RemoveGame_Bless.sql
GO
-- 1.2.0:
:r .\Scripts\AddMessage_AocRoleMenu.sql
GO
-- 1.3.0
:r .\Scripts\MigrateGameTable_1.2.0_1.3.0.sql
GO
-- 1.5.0:
:r .\Scripts\FillGameTable_IncludeInGuildMembersStatisticColumn.sql
GO
-- x.y.z:
*/