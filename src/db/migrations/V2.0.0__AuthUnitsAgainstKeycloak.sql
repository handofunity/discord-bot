ALTER TABLE config.units_endpoint
    DROP COLUMN secret,
    ADD COLUMN keycloak_endpoint_id INT NULL;

WITH cte AS (SELECT ke.keycloak_endpoint_id
             FROM config.keycloak_endpoint ke
             LIMIT 1)
UPDATE config.units_endpoint
SET keycloak_endpoint_id = cte.keycloak_endpoint_id
FROM cte;

ALTER TABLE config.units_endpoint
    ALTER COLUMN keycloak_endpoint_id SET NOT NULL;