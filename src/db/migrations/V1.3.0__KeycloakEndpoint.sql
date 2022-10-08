CREATE TABLE config.keycloak_endpoint
(
	keycloak_endpoint_id	INT             NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
	base_url				VARCHAR(256)    NOT NULL,
	access_token_url		VARCHAR(256)	NOT NULL,
	client_id				VARCHAR(128)	NOT NULL,
	client_secret			VARCHAR(128)    NOT NULL,
	realm                   VARCHAR(128)    NOT NULL,
	CONSTRAINT uq_base_url_realm UNIQUE (base_url, realm)
);