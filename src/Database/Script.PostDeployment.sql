﻿:r .\Scripts\CreateUser.sql
GO
:r .\Scripts\GrantPermissions.sql
GO
-- Scripts required for release start here
-- 0.2.0:
:r .\Scripts\AddMessage_FirstServerJoinWelcome.sql
GO
-- 0.3.0: