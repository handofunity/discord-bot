CREATE TABLE hou.auction_bot_sync (
    discord_user_id NUMERIC(20) PRIMARY KEY,
    last_change TIMESTAMPTZ NOT NULL,
    heritage_tokens BIGINT NOT NULL
);
