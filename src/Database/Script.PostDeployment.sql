-------------------------------------------
-------------------------------------------
----- GENERAL POST DEPLOYMENT SCRIPTS -----
-------------------------------------------
-------------------------------------------

:r .\Scripts\CreatePrimaryUser.sql
GO
:r .\Scripts\GrantPermissionsToPrimaryUser.sql
GO
:r .\Scripts\CreateHangFireUser.sql
GO
:r .\Scripts\GrantPermissionsToHangFireUser.sql
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
-- 2.2.1
:r .\Scripts\RemoveMessage_FirstServerJoinWelcome.sql
GO