CREATE TABLE config.discord_mapping
(
    discord_mapping_key VARCHAR(64)     NOT NULL PRIMARY KEY,
    discord_id          DECIMAL(20, 0)	NOT NULL
);

CREATE TABLE config.message
(
	message_id  INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	name		VARCHAR(128)    NOT NULL,
	description VARCHAR(512)    NOT NULL,
	content     VARCHAR(2000)   NOT NULL
);

CREATE UNIQUE INDEX idx_message_name_inc_content
    ON config.message (name ASC)
	INCLUDE(content);

CREATE TABLE hou.user
(
    user_id         INT				NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    discord_user_id DECIMAL(20, 0)  NOT NULL
);

CREATE UNIQUE INDEX idx_user_discord_user_id
    ON hou.user (discord_user_id ASC);

CREATE TABLE hou.user_info
(
	user_id     INT                         NOT NULL PRIMARY KEY,
	last_seen   TIMESTAMP WITH TIME ZONE    NOT NULL,
	CONSTRAINT fk_user_info_user FOREIGN KEY (user_id) REFERENCES hou.User (user_id)
);

CREATE TABLE hou.vacation
(
	vacation_id	INT				NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	user_id		INT				NOT NULL,
	start_date	DATE			NOT NULL,
	end_date	DATE			NOT NULL,
    note		VARCHAR(1024)	    NULL,
	CONSTRAINT fk_vacation_user FOREIGN KEY (user_id) REFERENCES hou.user (user_id)
);

CREATE TABLE config.game
(
    game_id                             SMALLINT                    NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    primary_game_discord_role_id        DECIMAL(20, 0)              NOT NULL,
    modified_by_user_id                 INT                         NOT NULL,
    modified_at_timestamp               TIMESTAMP WITH TIME ZONE    NOT NULL,
    include_in_guild_members_statistic  BOOLEAN                     NOT NULL,
    include_in_games_menu               BOOLEAN                     NOT NULL,
    game_interest_role_id               DECIMAL(20, 0)              NULL,
    CONSTRAINT uq_game_primary_game_discord_role_id UNIQUE (primary_game_discord_role_id),
    CONSTRAINT fk_game_user FOREIGN KEY (modified_by_user_id) REFERENCES hou.user (user_id)
);

CREATE UNIQUE INDEX idx_game_not_null_game_interest_role_id ON config.game (game_interest_role_id)
    WHERE game_interest_role_id IS NOT NULL;

CREATE TABLE config.game_role
(
	game_role_id            SMALLINT                    NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	discord_role_id         DECIMAL(20,0)               NOT NULL,
	game_id                 SMALLINT                    NOT NULL,
	modified_by_user_id     INT                         NOT NULL,
	modified_at_timestamp   TIMESTAMP WITH TIME ZONE    NOT NULL,
	CONSTRAINT uq_game_role_discord_role_id UNIQUE (discord_role_id),
	CONSTRAINT fk_game_role_game_game_id FOREIGN KEY (game_id) REFERENCES config.game (game_id),
	CONSTRAINT fk_game_role_user_modified_by_user_id FOREIGN KEY (modified_by_user_id) REFERENCES hou.user (user_id)
);

CREATE TABLE config.units_endpoint
(
	units_endpoint_id			    INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	base_address				    VARCHAR(256)	NOT NULL,
	secret					        VARCHAR(128)	NOT NULL,
	connect_to_rest_api			    BOOLEAN         NOT NULL,
	connect_to_notifications_hub    BOOLEAN         NOT NULL,
	CONSTRAINT uq_base_address UNIQUE (base_address)
);

CREATE TABLE config.desired_time_zone
(
	desired_time_zone_key   VARCHAR(128)    NOT NULL PRIMARY KEY,
	invariant_display_name  VARCHAR(1024)   NOT NULL
);

CREATE TABLE config.spam_protected_channel
(
	spam_protected_channel_id   DECIMAL(20, 0)  NOT NULL PRIMARY KEY,
	soft_cap                    INT             NOT NULL,
	hard_cap                    INT             NOT NULL
);

CREATE TABLE config.scheduled_reminder
(
	scheduled_reminder_id	INT				NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	cron_schedule			VARCHAR(64)		NOT NULL,
	discord_channel_id      DECIMAL(20, 0)	NOT NULL,
	text					VARCHAR(2048)	NOT NULL
);

CREATE TABLE config.scheduled_reminder_mention
(
	scheduled_reminder_mention_id	INT				NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	scheduled_reminder_id			INT				NOT NULL,
	discord_user_id					DECIMAL(20, 0)	NULL,
	discord_role_id					DECIMAL(20, 0)	NULL,
	CONSTRAINT fk_scheduled_reminder_mention_scheduled_reminder FOREIGN KEY (scheduled_reminder_id) REFERENCES config.scheduled_reminder (scheduled_reminder_id)
);