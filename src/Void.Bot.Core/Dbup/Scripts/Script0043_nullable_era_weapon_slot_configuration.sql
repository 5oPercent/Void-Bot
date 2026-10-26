ALTER TABLE era_weapon_template
    DROP FOREIGN KEY fk_era_weapon_slot_configuration;

ALTER TABLE era_weapon_template
    MODIFY COLUMN slot_configuration_id CHAR(36) NULL;

ALTER TABLE era_weapon_template
    ADD CONSTRAINT fk_era_weapon_slot_configuration FOREIGN KEY (slot_configuration_id) REFERENCES era_weapon_slot_configuration(id);
