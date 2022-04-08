CREATE TABLE hou.user_birthday
(
	user_id     INT         NOT NULL PRIMARY KEY,
	month       SMALLINT    NOT NULL,
    day         SMALLINT    NOT NULL,
	CONSTRAINT fk_user_birthday_user FOREIGN KEY (user_id) REFERENCES hou.User (user_id)
);