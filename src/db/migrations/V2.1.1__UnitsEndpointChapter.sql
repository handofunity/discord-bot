ALTER TABLE config.units_endpoint
ADD COLUMN chapter VARCHAR(32) NULL;

UPDATE config.units_endpoint
SET
    chapter = units_endpoint_id;

ALTER TABLE config.units_endpoint
ALTER COLUMN chapter
SET NOT NULL;

ALTER TABLE config.units_endpoint
ADD CONSTRAINT uq_chapter UNIQUE (chapter);