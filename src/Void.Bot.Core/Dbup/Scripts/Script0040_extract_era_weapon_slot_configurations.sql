-- No existing weapon business data: schema-only upgrade, no legacy data conversion or seed configuration.
-- Fail before changing any table if the empty-data precondition no longer holds.
DROP PROCEDURE IF EXISTS era_require_empty_weapon_data;
DELIMITER $$
CREATE PROCEDURE era_require_empty_weapon_data()
BEGIN
    IF EXISTS (SELECT 1 FROM era_weapon_template LIMIT 1) OR
        EXISTS (SELECT 1 FROM era_weapon_mod_definition LIMIT 1) OR
        EXISTS (SELECT 1 FROM era_weapon_mod_candidate LIMIT 1) OR
        EXISTS (SELECT 1 FROM era_weapon_mod_selection LIMIT 1) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Weapon slot schema upgrade requires empty weapon tables';
    END IF;
END$$
DELIMITER ;
CALL era_require_empty_weapon_data();
DROP PROCEDURE era_require_empty_weapon_data;

CREATE TABLE era_weapon_slot_type (
    id CHAR(36) NOT NULL PRIMARY KEY, name VARCHAR(128) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_slot_configuration (
    id CHAR(36) NOT NULL PRIMARY KEY, name VARCHAR(128) NOT NULL, note VARCHAR(2048) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_configuration_slot (
    id CHAR(36) NOT NULL PRIMARY KEY, configuration_id CHAR(36) NOT NULL,
    name VARCHAR(128) NOT NULL, slot_type_id CHAR(36) NOT NULL,
    candidate_count INT NOT NULL, display_order INT NOT NULL,
    CONSTRAINT fk_era_configuration_slot_configuration FOREIGN KEY (configuration_id) REFERENCES era_weapon_slot_configuration(id),
    CONSTRAINT fk_era_configuration_slot_type FOREIGN KEY (slot_type_id) REFERENCES era_weapon_slot_type(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_configuration_mod_pool (
    slot_id CHAR(36) NOT NULL, mod_definition_id CHAR(36) NOT NULL, weight INT NOT NULL,
    PRIMARY KEY (slot_id, mod_definition_id),
    CONSTRAINT fk_era_configuration_pool_slot FOREIGN KEY (slot_id) REFERENCES era_weapon_configuration_slot(id),
    CONSTRAINT fk_era_configuration_pool_mod FOREIGN KEY (mod_definition_id) REFERENCES era_weapon_mod_definition(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

ALTER TABLE era_weapon_template ADD COLUMN slot_configuration_id CHAR(36) NOT NULL,
    ADD CONSTRAINT fk_era_weapon_slot_configuration FOREIGN KEY (slot_configuration_id) REFERENCES era_weapon_slot_configuration(id);
ALTER TABLE era_weapon_mod_definition CHANGE COLUMN slot slot_type_id CHAR(36) NOT NULL,
    ADD CONSTRAINT fk_era_weapon_mod_slot_type FOREIGN KEY (slot_type_id) REFERENCES era_weapon_slot_type(id);
ALTER TABLE era_weapon_mod_selection DROP FOREIGN KEY fk_era_weapon_selection_candidate;
ALTER TABLE era_weapon_mod_candidate DROP INDEX ux_era_weapon_candidate_identity, DROP INDEX ux_era_weapon_candidate_definition,
    CHANGE COLUMN slot slot_id CHAR(36) NOT NULL,
    ADD UNIQUE KEY ux_era_weapon_candidate_identity (equipment_id, slot_id, candidate_id),
    ADD UNIQUE KEY ux_era_weapon_candidate_definition (equipment_id, slot_id, mod_definition_id);
ALTER TABLE era_weapon_mod_selection DROP PRIMARY KEY,
    CHANGE COLUMN slot slot_id CHAR(36) NOT NULL,
    ADD COLUMN name VARCHAR(128) NOT NULL, ADD COLUMN slot_type_id CHAR(36) NOT NULL, ADD COLUMN display_order INT NOT NULL,
    ADD PRIMARY KEY (equipment_id, slot_id),
    ADD CONSTRAINT fk_era_weapon_selection_type FOREIGN KEY (slot_type_id) REFERENCES era_weapon_slot_type(id),
    ADD CONSTRAINT fk_era_weapon_selection_candidate FOREIGN KEY (equipment_id, slot_id, selected_candidate_id)
        REFERENCES era_weapon_mod_candidate(equipment_id, slot_id, candidate_id);

DROP TABLE era_weapon_template_mod_pool;
DROP TABLE era_weapon_template_mod_slot;
