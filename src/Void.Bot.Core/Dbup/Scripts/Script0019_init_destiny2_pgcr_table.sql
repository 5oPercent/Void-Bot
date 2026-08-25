CREATE TABLE destiny2_pgcr (
    instance_id BIGINT NOT NULL PRIMARY KEY,
    period DATETIME NOT NULL,
    reference_id BIGINT UNSIGNED NOT NULL,
    starting_phase_index INT NULL,
    started_from_beginning BOOLEAN NULL,
    data_version INT NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NULL
);

CREATE TABLE destiny2_pgcr_player (
    id CHAR(36) NOT NULL PRIMARY KEY,
    instance_id BIGINT NOT NULL,
    character_id VARCHAR(64) NOT NULL,
    membership_type INT NULL,
    membership_id VARCHAR(64) NULL,
    completed BOOLEAN NULL,
    start_seconds BIGINT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NULL
);

CREATE UNIQUE INDEX IX_destiny2_pgcr_player_instance_character
    ON destiny2_pgcr_player (instance_id, character_id);

CREATE INDEX IX_destiny2_pgcr_player_character
    ON destiny2_pgcr_player (character_id);

CREATE INDEX IX_destiny2_pgcr_player_membership
    ON destiny2_pgcr_player (membership_type, membership_id);

CREATE INDEX IX_destiny2_pgcr_data_version
    ON destiny2_pgcr (data_version);
