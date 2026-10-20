ALTER TABLE `era_reward_event`
    ADD COLUMN `world_instance_id` CHAR(36) NULL,
    ADD KEY `ix_era_reward_event_world` (`world_instance_id`),
    ADD CONSTRAINT `fk_era_reward_event_world` FOREIGN KEY (`world_instance_id`) REFERENCES `era_world` (`id`);

CREATE TABLE `era_equipment_drop` (
    `id` CHAR(36) NOT NULL,
    `equipment_id` CHAR(36) NOT NULL,
    `active_equipment_id` CHAR(36) NULL,
    `world_instance_id` CHAR(36) NOT NULL,
    `private_owner_player_id` CHAR(36) NULL,
    `dropped_by_player_id` CHAR(36) NULL,
    `status` INT NOT NULL,
    `version` BIGINT NOT NULL,
    `x` DOUBLE NOT NULL,
    `y` DOUBLE NOT NULL,
    `z` DOUBLE NOT NULL,
    `yaw_radians` DOUBLE NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    `updated_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_equipment_drop_active` (`active_equipment_id`),
    KEY `ix_era_equipment_drop_visible` (`world_instance_id`, `private_owner_player_id`, `status`),
    KEY `ix_era_equipment_drop_equipment` (`equipment_id`),
    KEY `ix_era_equipment_drop_private_owner` (`private_owner_player_id`),
    KEY `ix_era_equipment_drop_dropped_by` (`dropped_by_player_id`),
    CONSTRAINT `fk_era_equipment_drop_equipment` FOREIGN KEY (`equipment_id`) REFERENCES `era_equipment_instance` (`id`),
    CONSTRAINT `fk_era_equipment_drop_world` FOREIGN KEY (`world_instance_id`) REFERENCES `era_world` (`id`),
    CONSTRAINT `fk_era_equipment_drop_private_owner` FOREIGN KEY (`private_owner_player_id`) REFERENCES `era_player` (`id`),
    CONSTRAINT `fk_era_equipment_drop_dropped_by` FOREIGN KEY (`dropped_by_player_id`) REFERENCES `era_player` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_equipment_share` (
    `equipment_id` CHAR(36) NOT NULL,
    `allowed_player_ids_payload` TEXT NOT NULL,
    `share_until` DATETIME(6) NOT NULL,
    PRIMARY KEY (`equipment_id`),
    CONSTRAINT `fk_era_equipment_share_equipment` FOREIGN KEY (`equipment_id`) REFERENCES `era_equipment_instance` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
