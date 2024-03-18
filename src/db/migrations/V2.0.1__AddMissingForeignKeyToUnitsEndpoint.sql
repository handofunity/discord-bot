ALTER TABLE config.units_endpoint
    ADD FOREIGN KEY (keycloak_endpoint_id) REFERENCES config.keycloak_endpoint (keycloak_endpoint_id);