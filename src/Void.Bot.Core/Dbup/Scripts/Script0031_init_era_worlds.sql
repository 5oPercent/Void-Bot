-- 队伍提交与世界写入共用锁行；ActiveWorldId 由世界事务维护，不复制 Redis 权限。
CREATE TABLE `era_world_team_state` (
    `team_id` CHAR(36) NOT NULL,
    `active_world_id` CHAR(36) NULL,
    PRIMARY KEY (`team_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_world` (
    `id` CHAR(36) NOT NULL,
    `team_id` CHAR(36) NOT NULL,
    `creation_request_id` CHAR(36) NOT NULL,
    `created_by_player_id` CHAR(36) NOT NULL,
    `creation_member_id` CHAR(36) NOT NULL,
    `creation_authority_epoch` BIGINT NOT NULL,
    `world_definition_id` VARCHAR(128) NOT NULL,
    `state` INT NOT NULL,
    `host_player_id` CHAR(36) NOT NULL,
    `authority_epoch` BIGINT NOT NULL,
    `version` BIGINT NOT NULL,
    `is_equipped_operations_restricted` TINYINT(1) NOT NULL,
    `restriction_version` BIGINT NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    `ended_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_world_creation_request` (`team_id`, `creation_request_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_world_member` (
    `world_id` CHAR(36) NOT NULL,
    `player_id` CHAR(36) NOT NULL,
    `character_id` CHAR(36) NOT NULL,
    `member_id` CHAR(36) NOT NULL,
    `is_continuing` TINYINT(1) NOT NULL,
    `active_player_id` CHAR(36) NULL,
    `active_character_id` CHAR(36) NULL,
    PRIMARY KEY (`world_id`, `player_id`),
    UNIQUE KEY `ux_era_world_active_player` (`active_player_id`),
    UNIQUE KEY `ux_era_world_active_character` (`active_character_id`),
    KEY `ix_era_world_member_character_player` (`character_id`, `player_id`),
    CONSTRAINT `fk_era_world_member_world` FOREIGN KEY (`world_id`) REFERENCES `era_world` (`id`),
    CONSTRAINT `fk_era_world_member_character_owner` FOREIGN KEY (`character_id`, `player_id`)
        REFERENCES `era_character` (`id`, `player_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
