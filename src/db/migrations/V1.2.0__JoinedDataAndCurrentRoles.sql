ALTER TABLE hou.user_info
    ADD COLUMN joined_date TIMESTAMP WITH TIME ZONE NULL;

UPDATE hou.user_info
SET joined_date = TO_TIMESTAMP(946684800); -- 2000-01-01 00:00:00 UTC

ALTER TABLE hou.user_info
    ALTER COLUMN joined_date SET NOT NULL;

ALTER TABLE hou.user_info
    ADD COLUMN current_roles VARCHAR(32768) NULL;