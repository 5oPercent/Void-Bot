-- Armor template upgrade requires an empty database; no legacy asset conversion or sample configuration.
DROP PROCEDURE IF EXISTS era_require_empty_armor_data;
DELIMITER $$
CREATE PROCEDURE era_require_empty_armor_data()
BEGIN
    IF EXISTS (SELECT 1 FROM era_armor_instance_data LIMIT 1) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Armor template upgrade requires empty armor assets';
    END IF;
END$$
DELIMITER ;
CALL era_require_empty_armor_data();
DROP PROCEDURE era_require_empty_armor_data;

CREATE TABLE era_armor_slot_type (
    id CHAR(36) NOT NULL PRIMARY KEY, name VARCHAR(128) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_armor_slot_configuration (
    id CHAR(36) NOT NULL PRIMARY KEY, name VARCHAR(128) NOT NULL, note VARCHAR(2048) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_armor_template (
    id CHAR(36) NOT NULL PRIMARY KEY, name VARCHAR(128) NOT NULL, icon VARCHAR(256) NOT NULL,
    note VARCHAR(2048) NOT NULL, is_generation_enabled TINYINT(1) NOT NULL,
    slot_configuration_id CHAR(36) NULL, compatible_slots_payload TEXT NOT NULL,
    CONSTRAINT fk_era_armor_template_scheme FOREIGN KEY (slot_configuration_id) REFERENCES era_armor_slot_configuration(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_armor_mod_definition (
    id CHAR(36) NOT NULL PRIMARY KEY, name VARCHAR(128) NOT NULL, slot_type_id CHAR(36) NOT NULL,
    description VARCHAR(2048) NOT NULL, icon VARCHAR(256) NOT NULL,
    CONSTRAINT fk_era_armor_mod_type FOREIGN KEY (slot_type_id) REFERENCES era_armor_slot_type(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_armor_configuration_slot (
    id CHAR(36) NOT NULL PRIMARY KEY, configuration_id CHAR(36) NOT NULL,
    name VARCHAR(128) NOT NULL, slot_type_id CHAR(36) NOT NULL, candidate_count INT NOT NULL, display_order INT NOT NULL,
    CONSTRAINT fk_era_armor_slot_scheme FOREIGN KEY (configuration_id) REFERENCES era_armor_slot_configuration(id),
    CONSTRAINT fk_era_armor_slot_type FOREIGN KEY (slot_type_id) REFERENCES era_armor_slot_type(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_armor_mod_stat_modifier (
    mod_definition_id CHAR(36) NOT NULL, `sequence` INT NOT NULL, stat INT NOT NULL, operation INT NOT NULL, `value` DOUBLE NOT NULL,
    PRIMARY KEY (mod_definition_id, `sequence`),
    CONSTRAINT fk_era_armor_modifier_definition FOREIGN KEY (mod_definition_id) REFERENCES era_armor_mod_definition(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_armor_configuration_mod_pool (
    slot_id CHAR(36) NOT NULL, mod_definition_id CHAR(36) NOT NULL, weight INT NOT NULL,
    PRIMARY KEY (slot_id, mod_definition_id),
    CONSTRAINT fk_era_armor_pool_slot FOREIGN KEY (slot_id) REFERENCES era_armor_configuration_slot(id),
    CONSTRAINT fk_era_armor_pool_mod FOREIGN KEY (mod_definition_id) REFERENCES era_armor_mod_definition(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_armor_mod_candidate (
    candidate_id CHAR(36) NOT NULL PRIMARY KEY, equipment_id CHAR(36) NOT NULL, slot_id CHAR(36) NOT NULL,
    mod_definition_id CHAR(36) NOT NULL, display_order INT NOT NULL,
    UNIQUE KEY ux_era_armor_candidate_identity (equipment_id, slot_id, candidate_id),
    UNIQUE KEY ux_era_armor_candidate_definition (equipment_id, slot_id, mod_definition_id),
    CONSTRAINT fk_era_armor_candidate_equipment FOREIGN KEY (equipment_id) REFERENCES era_equipment_instance(id),
    CONSTRAINT fk_era_armor_candidate_mod FOREIGN KEY (mod_definition_id) REFERENCES era_armor_mod_definition(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_armor_mod_selection (
    equipment_id CHAR(36) NOT NULL, slot_id CHAR(36) NOT NULL, name VARCHAR(128) NOT NULL,
    slot_type_id CHAR(36) NOT NULL, display_order INT NOT NULL, selected_candidate_id CHAR(36) NOT NULL,
    PRIMARY KEY (equipment_id, slot_id),
    CONSTRAINT fk_era_armor_selection_type FOREIGN KEY (slot_type_id) REFERENCES era_armor_slot_type(id),
    CONSTRAINT fk_era_armor_selection_candidate FOREIGN KEY (equipment_id, slot_id, selected_candidate_id)
        REFERENCES era_armor_mod_candidate(equipment_id, slot_id, candidate_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- The global row holds publication metadata only; armor templates now live in relational tables.
UPDATE era_equipment_configuration
SET payload = '{"SchemaVersion":2}', version = version + 1;
