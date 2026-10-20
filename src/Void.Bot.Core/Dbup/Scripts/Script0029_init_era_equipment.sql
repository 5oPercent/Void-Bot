-- 仅建立存储，不生成装备或给已有角色补发资产。
-- 现有 MySQL 配置兼容 8.0.3，不依赖 8.0.16 后才强制执行的 CHECK 约束。
ALTER TABLE `era_character`
    ADD COLUMN `equipment_version` BIGINT NOT NULL DEFAULT 0,
    ADD UNIQUE KEY `ux_era_character_owner` (`id`, `player_id`);

CREATE TABLE `era_equipment_configuration` (
    `id` CHAR(36) NOT NULL,
    `version` BIGINT NOT NULL,
    `payload` MEDIUMTEXT NOT NULL,
    `last_modified_by` CHAR(36) NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 沿用固定发布行；空目录是有效初始状态，不预设未经确认的正式物品。
INSERT INTO `era_equipment_configuration` (`id`, `version`, `payload`)
VALUES ('30000000-0000-0000-0000-000000000001', 1,
    '{"SchemaVersion":1,"Definitions":[]}');

CREATE TABLE `era_equipment_instance` (
    `id` CHAR(36) NOT NULL,
    `definition_id` VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
    `definition_version` BIGINT NOT NULL,
    `equipment_type` INT NOT NULL,
    `grade` INT NOT NULL,
    `owner_player_id` CHAR(36) NULL,
    `character_id` CHAR(36) NULL,
    `location` INT NOT NULL,
    `inventory_slot` INT NOT NULL,
    `equipped_slot` INT NULL,
    `version` BIGINT NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_equipment_equipped_slot` (`character_id`, `equipped_slot`),
    KEY `ix_era_equipment_character_inventory` (`character_id`, `location`, `inventory_slot`),
    KEY `ix_era_equipment_owner_location` (`owner_player_id`, `location`),
    KEY `ix_era_equipment_character_owner` (`character_id`, `owner_player_id`),
    CONSTRAINT `fk_era_equipment_owner` FOREIGN KEY (`owner_player_id`) REFERENCES `era_player` (`id`),
    CONSTRAINT `fk_era_equipment_character_owner` FOREIGN KEY (`character_id`, `owner_player_id`)
        REFERENCES `era_character` (`id`, `player_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_armor_instance_data` (
    `equipment_id` CHAR(36) NOT NULL,
    `generation_rule_version` INT NOT NULL,
    `strength` INT NOT NULL,
    `survival` INT NOT NULL,
    `mobility` INT NOT NULL,
    `focus` INT NOT NULL,
    `efficiency` INT NOT NULL,
    `resistance` INT NOT NULL,
    PRIMARY KEY (`equipment_id`),
    CONSTRAINT `fk_era_armor_equipment` FOREIGN KEY (`equipment_id`) REFERENCES `era_equipment_instance` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 后续写入必须在同一事务内维护以下组合及每分区容量，不能仅凭上述可空外键判断：
-- Ground / Destroyed: owner_player_id、character_id、equipped_slot 均为空。
-- Inventory: owner_player_id、character_id 非空，equipped_slot 为空。
-- Equipped: owner_player_id、character_id、equipped_slot 均非空。
-- Warehouse: owner_player_id 非空，character_id、equipped_slot 为空。
