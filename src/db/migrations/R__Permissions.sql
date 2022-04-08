DO LANGUAGE plpgsql $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles AS r WHERE r.rolname = 'hou_guildbot')
    THEN
        RAISE EXCEPTION 'Missing role --> hou_guildbot';
    ELSE
        RAISE INFO 'Revoking existing permissions for hou_guildbot ...';
        REVOKE ALL ON ALL TABLES IN SCHEMA config FROM hou_guildbot;
        REVOKE ALL ON ALL TABLES IN SCHEMA hou FROM hou_guildbot;

        RAISE INFO 'Adding permissions for hou_guildbot ...';
        GRANT USAGE ON SCHEMA config TO hou_guildbot;
        GRANT USAGE ON SCHEMA hou TO hou_guildbot;
        GRANT SELECT, INSERT ON hou.user TO hou_guildbot;
        GRANT SELECT, INSERT, UPDATE, DELETE ON hou.user_info TO hou_guildbot;
        GRANT SELECT, INSERT, UPDATE, DELETE ON hou.user_birthday TO hou_guildbot;
        GRANT SELECT, INSERT, DELETE ON hou.vacation TO hou_guildbot;
        GRANT SELECT, INSERT, UPDATE, DELETE ON config.game TO hou_guildbot;
        GRANT SELECT, INSERT, UPDATE, DELETE ON config.game_role TO hou_guildbot;
        GRANT SELECT, INSERT ON config.scheduled_reminder TO hou_guildbot;
        GRANT SELECT, INSERT ON config.scheduled_reminder_mention TO hou_guildbot;
        GRANT SELECT ON config.message TO hou_guildbot;
        GRANT SELECT ON config.discord_mapping TO hou_guildbot;
        GRANT SELECT ON config.spam_protected_channel TO hou_guildbot;
        GRANT SELECT ON config.desired_time_zone TO hou_guildbot;
        GRANT SELECT ON config.units_endpoint TO hou_guildbot;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_roles AS r WHERE r.rolname = 'hang_fire_user')
    THEN
        RAISE EXCEPTION 'Missing role --> hang_fire_user';
    ELSE
        RAISE INFO 'Revoking existing permissions for hang_fire_user ...';
        REVOKE ALL ON ALL TABLES IN SCHEMA hang_fire FROM hang_fire_user;

        RAISE INFO 'Adding permissions for hang_fire_user ...';
        GRANT USAGE ON SCHEMA hang_fire TO hang_fire_user;
        GRANT ALL ON SCHEMA hang_fire TO hang_fire_user;
        GRANT ALL ON ALL TABLES IN SCHEMA hang_fire TO hang_fire_user;
    END IF;
END
$$;