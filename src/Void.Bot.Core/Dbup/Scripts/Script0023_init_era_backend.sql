CREATE TABLE `era_configuration` (
    `id` CHAR(36) NOT NULL,
    `config_key` VARCHAR(128) NOT NULL,
    `value_type` VARCHAR(32) NOT NULL,
    `value` TEXT NOT NULL,
    `description` VARCHAR(256) NOT NULL,
    `config_version` BIGINT NOT NULL,
    `updated_at` DATETIME(6) NULL,
    `last_modified_by` CHAR(36) NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_configuration_key` (`config_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `era_configuration` (
    `id`,
    `config_key`,
    `value_type`,
    `value`,
    `description`,
    `config_version`)
VALUES
    ('10000000-0000-0000-0000-000000000001', 'player.attribute.minimum', 'decimal', '0', '玩家基础属性下限', 1),
    ('10000000-0000-0000-0000-000000000002', 'player.attribute.maximum', 'decimal', '100', '玩家基础属性上限', 1),
    ('10000000-0000-0000-0000-000000000003', 'player.strength.base', 'decimal', '0', '基础力量', 1),
    ('10000000-0000-0000-0000-000000000004', 'player.constitution.base', 'decimal', '0', '基础体质', 1),
    ('10000000-0000-0000-0000-000000000005', 'player.agility.base', 'decimal', '0', '基础敏捷', 1),
    ('10000000-0000-0000-0000-000000000006', 'player.health.base', 'decimal', '125', '基础生命值', 1),
    ('10000000-0000-0000-0000-000000000007', 'player.strength.full_bonus_percent', 'decimal', '50', '满力量武器攻击力加成百分比', 1),
    ('10000000-0000-0000-0000-000000000008', 'player.constitution.full_bonus_percent', 'decimal', '80', '满体质生命值加成百分比', 1),
    ('10000000-0000-0000-0000-000000000009', 'player.agility.full_bonus_percent', 'decimal', '10', '满敏捷移动速度加成百分比', 1),
    ('10000000-0000-0000-0000-000000000010', 'warehouse.default_capacity', 'integer', '300', '新玩家默认共享仓库容量', 1);

CREATE TABLE `era_player` (
    `id` CHAR(36) NOT NULL,
    `warehouse_capacity` INT NOT NULL,
    `version` BIGINT NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_player_account_link` (
    `id` CHAR(36) NOT NULL,
    `account_id` CHAR(36) NOT NULL,
    `player_id` CHAR(36) NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_player_account_link_account` (`account_id`),
    UNIQUE KEY `ux_era_player_account_link_player` (`player_id`),
    CONSTRAINT `fk_era_player_account_link_account` FOREIGN KEY (`account_id`) REFERENCES `user_account` (`id`),
    CONSTRAINT `fk_era_player_account_link_player` FOREIGN KEY (`player_id`) REFERENCES `era_player` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_character` (
    `id` CHAR(36) NOT NULL,
    `player_id` CHAR(36) NOT NULL,
    `creation_request_id` CHAR(36) NOT NULL,
    `character_definition_id` VARCHAR(128) NOT NULL,
    `display_name` VARCHAR(64) NOT NULL,
    `is_disabled` TINYINT(1) NOT NULL DEFAULT 0,
    `version` BIGINT NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_character_creation_request` (`player_id`, `creation_request_id`),
    KEY `ix_era_character_player` (`player_id`),
    CONSTRAINT `fk_era_character_player` FOREIGN KEY (`player_id`) REFERENCES `era_player` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_capacity_change` (
    `id` CHAR(36) NOT NULL,
    `player_id` CHAR(36) NOT NULL,
    `request_id` CHAR(36) NOT NULL,
    `operator_id` CHAR(36) NOT NULL,
    `is_adjustment` TINYINT(1) NOT NULL,
    `requested_value` INT NOT NULL,
    `expected_version` BIGINT NOT NULL,
    `reason` VARCHAR(512) NOT NULL,
    `previous_capacity` INT NOT NULL,
    `warehouse_capacity` INT NOT NULL,
    `player_version` BIGINT NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_capacity_request` (`player_id`, `request_id`),
    CONSTRAINT `fk_era_capacity_player` FOREIGN KEY (`player_id`) REFERENCES `era_player` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
