-- 内部奖励能力；空配置不会给现有玩家发奖，也不开放客户端来源接口。
CREATE TABLE `era_reward_configuration` (
    `id` CHAR(36) NOT NULL,
    `version` BIGINT NOT NULL,
    `payload` MEDIUMTEXT NOT NULL,
    `last_modified_by` CHAR(36) NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `era_reward_configuration` (`id`, `version`, `payload`)
VALUES ('40000000-0000-0000-0000-000000000001', 1, '{"SchemaVersion":1,"Nodes":[],"Pools":[]}');

CREATE TABLE `era_reward_scope` (
    `scope_id` CHAR(36) NOT NULL,
    PRIMARY KEY (`scope_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_reward_event` (
    `id` CHAR(36) NOT NULL,
    `scope_id` CHAR(36) NOT NULL,
    `source_type` INT NOT NULL,
    `source_instance_id` CHAR(36) NOT NULL,
    `source_definition_id` VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
    `trigger_index` INT NOT NULL,
    `reward_node_id` VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
    `request_payload` TEXT NOT NULL,
    `configuration_version` BIGINT NOT NULL,
    `manifest_version` BIGINT NOT NULL,
    `accepted_at` DATETIME(6) NOT NULL,
    `plan_payload` MEDIUMTEXT NOT NULL,
    `result_payload` MEDIUMTEXT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_reward_event_scope` (`id`, `scope_id`),
    UNIQUE KEY `ux_era_reward_event_identity` (`scope_id`, `source_type`, `source_instance_id`, `trigger_index`, `reward_node_id`),
    CONSTRAINT `fk_era_reward_event_scope` FOREIGN KEY (`scope_id`) REFERENCES `era_reward_scope` (`scope_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_reward_request` (
    `scope_id` CHAR(36) NOT NULL,
    `request_id` CHAR(36) NOT NULL,
    `event_id` CHAR(36) NOT NULL,
    PRIMARY KEY (`scope_id`, `request_id`),
    KEY `ix_era_reward_request_event_scope` (`event_id`, `scope_id`),
    CONSTRAINT `fk_era_reward_request_event_scope` FOREIGN KEY (`event_id`, `scope_id`) REFERENCES `era_reward_event` (`id`, `scope_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_reward_grant` (
    `id` CHAR(36) NOT NULL,
    `event_id` CHAR(36) NOT NULL,
    `player_id` CHAR(36) NOT NULL,
    `character_id` CHAR(36) NOT NULL,
    `granted_at` DATETIME(6) NOT NULL,
    `result_payload` MEDIUMTEXT NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_reward_grant_player` (`event_id`, `player_id`),
    KEY `ix_era_reward_grant_character_owner` (`character_id`, `player_id`),
    CONSTRAINT `fk_era_reward_grant_event` FOREIGN KEY (`event_id`) REFERENCES `era_reward_event` (`id`),
    CONSTRAINT `fk_era_reward_grant_character_owner` FOREIGN KEY (`character_id`, `player_id`) REFERENCES `era_character` (`id`, `player_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_reward_grant_equipment` (
    `grant_id` CHAR(36) NOT NULL,
    `equipment_id` CHAR(36) NOT NULL,
    PRIMARY KEY (`grant_id`, `equipment_id`),
    UNIQUE KEY `ux_era_reward_grant_equipment` (`equipment_id`),
    CONSTRAINT `fk_era_reward_grant_equipment_grant` FOREIGN KEY (`grant_id`) REFERENCES `era_reward_grant` (`id`),
    CONSTRAINT `fk_era_reward_grant_equipment_item` FOREIGN KEY (`equipment_id`) REFERENCES `era_equipment_instance` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
