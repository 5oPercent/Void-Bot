CREATE TABLE `era_equipment_operation_receipt` (
    `id` CHAR(36) NOT NULL,
    `player_id` CHAR(36) NOT NULL,
    `character_id` CHAR(36) NOT NULL,
    `request_id` CHAR(36) NOT NULL,
    `operation` INT NOT NULL,
    `request_payload` TEXT NOT NULL,
    `result_payload` MEDIUMTEXT NOT NULL,
    `created_at` DATETIME(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_era_equipment_operation_request` (`player_id`, `request_id`),
    KEY `ix_era_equipment_operation_character_owner` (`character_id`, `player_id`),
    CONSTRAINT `fk_era_equipment_operation_character_owner` FOREIGN KEY (`character_id`, `player_id`)
        REFERENCES `era_character` (`id`, `player_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
