CREATE TABLE `era_skill_configuration` (
    `id` CHAR(36) NOT NULL,
    `version` BIGINT NOT NULL,
    `payload` MEDIUMTEXT NOT NULL,
    `last_modified_by` CHAR(36) NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `era_skill_configuration` (`id`, `version`, `payload`)
VALUES ('20000000-0000-0000-0000-000000000001', 0,
    '{"Version":0,"SchemaVersion":1,"Systems":[],"Skills":[],"Buffs":[],"CommonTacticalSkills":[],"DisabledSkills":[]}');

CREATE TABLE `era_player_skill_profile` (
    `id` CHAR(36) NOT NULL,
    `player_id` CHAR(36) NOT NULL,
    `version` BIGINT NOT NULL,
    `payload` MEDIUMTEXT NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_player_skill_profile_player` (`player_id`),
    CONSTRAINT `fk_era_player_skill_profile_player` FOREIGN KEY (`player_id`) REFERENCES `era_player` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_player_learned_skill` (
    `id` CHAR(36) NOT NULL,
    `player_id` CHAR(36) NOT NULL,
    `skill_id` VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_player_learned_skill` (`player_id`, `skill_id`),
    CONSTRAINT `fk_era_player_learned_skill_player` FOREIGN KEY (`player_id`) REFERENCES `era_player` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
