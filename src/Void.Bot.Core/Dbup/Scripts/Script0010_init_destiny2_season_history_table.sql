CREATE TABLE destiny2_season_history (
    id CHAR(36) NOT NULL PRIMARY KEY,
    membership_type INT NOT NULL,
    membership_id VARCHAR(64) NOT NULL,
    season_hash BIGINT UNSIGNED NOT NULL,
    hunter_seconds BIGINT NOT NULL DEFAULT 0,
    titan_seconds BIGINT NOT NULL DEFAULT 0,
    warlock_seconds BIGINT NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IX_destiny2_season_history_player_season
    ON destiny2_season_history (membership_type, membership_id, season_hash);
