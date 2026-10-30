-- Instance naming changes with existing template data and no equipment instances. Template rows and references are preserved.
RENAME TABLE era_armor_instance_data TO era_armor_instance,
    era_armor_mod_candidate TO era_armor_instance_mod_candidate,
    era_armor_mod_selection TO era_armor_instance_mod_selection,
    era_weapon_mod_candidate TO era_weapon_instance_mod_candidate,
    era_weapon_mod_selection TO era_weapon_instance_mod_selection;

-- Drop only foreign keys involving renamed columns; primary and unique keys remain in place.
ALTER TABLE era_armor_instance DROP FOREIGN KEY fk_era_armor_equipment;
ALTER TABLE era_armor_instance_mod_selection DROP FOREIGN KEY fk_era_armor_selection_candidate;
ALTER TABLE era_armor_instance_mod_candidate DROP FOREIGN KEY fk_era_armor_candidate_equipment;
ALTER TABLE era_weapon_instance_mod_selection DROP FOREIGN KEY fk_era_weapon_selection_candidate;
ALTER TABLE era_weapon_instance_mod_candidate DROP FOREIGN KEY fk_era_weapon_candidate_equipment;
ALTER TABLE era_equipment_drop DROP FOREIGN KEY fk_era_equipment_drop_equipment;
ALTER TABLE era_equipment_share DROP FOREIGN KEY fk_era_equipment_share_equipment;
ALTER TABLE era_reward_grant_equipment DROP FOREIGN KEY fk_era_reward_grant_equipment_item;

ALTER TABLE era_equipment_instance
    CHANGE COLUMN definition_id template_id CHAR(36) NOT NULL,
    CHANGE COLUMN definition_version generation_manifest_version BIGINT NOT NULL;
ALTER TABLE era_armor_instance CHANGE COLUMN equipment_id equipment_instance_id CHAR(36) NOT NULL;
ALTER TABLE era_armor_instance_mod_candidate CHANGE COLUMN equipment_id equipment_instance_id CHAR(36) NOT NULL;
ALTER TABLE era_armor_instance_mod_selection CHANGE COLUMN equipment_id equipment_instance_id CHAR(36) NOT NULL;
ALTER TABLE era_weapon_instance_mod_candidate CHANGE COLUMN equipment_id equipment_instance_id CHAR(36) NOT NULL;
ALTER TABLE era_weapon_instance_mod_selection CHANGE COLUMN equipment_id equipment_instance_id CHAR(36) NOT NULL;
ALTER TABLE era_equipment_drop
    CHANGE COLUMN equipment_id equipment_instance_id CHAR(36) NOT NULL,
    CHANGE COLUMN active_equipment_id active_equipment_instance_id CHAR(36) NULL;
ALTER TABLE era_equipment_share CHANGE COLUMN equipment_id equipment_instance_id CHAR(36) NOT NULL;
ALTER TABLE era_reward_grant_equipment CHANGE COLUMN equipment_id equipment_instance_id CHAR(36) NOT NULL;

ALTER TABLE era_armor_instance
    ADD CONSTRAINT fk_era_armor_instance_equipment FOREIGN KEY (equipment_instance_id) REFERENCES era_equipment_instance(id);
ALTER TABLE era_armor_instance_mod_candidate
    ADD CONSTRAINT fk_era_armor_instance_candidate_equipment FOREIGN KEY (equipment_instance_id) REFERENCES era_equipment_instance(id);
ALTER TABLE era_armor_instance_mod_selection
    ADD CONSTRAINT fk_era_armor_instance_selection_candidate FOREIGN KEY (equipment_instance_id, slot_id, selected_candidate_id)
        REFERENCES era_armor_instance_mod_candidate(equipment_instance_id, slot_id, candidate_id);
ALTER TABLE era_weapon_instance_mod_candidate
    ADD CONSTRAINT fk_era_weapon_instance_candidate_equipment FOREIGN KEY (equipment_instance_id) REFERENCES era_equipment_instance(id);
ALTER TABLE era_weapon_instance_mod_selection
    ADD CONSTRAINT fk_era_weapon_instance_selection_candidate FOREIGN KEY (equipment_instance_id, slot_id, selected_candidate_id)
        REFERENCES era_weapon_instance_mod_candidate(equipment_instance_id, slot_id, candidate_id);
ALTER TABLE era_equipment_drop
    ADD CONSTRAINT fk_era_equipment_drop_instance FOREIGN KEY (equipment_instance_id) REFERENCES era_equipment_instance(id);
ALTER TABLE era_equipment_share
    ADD CONSTRAINT fk_era_equipment_share_instance FOREIGN KEY (equipment_instance_id) REFERENCES era_equipment_instance(id);
ALTER TABLE era_reward_grant_equipment
    ADD CONSTRAINT fk_era_reward_grant_equipment_instance FOREIGN KEY (equipment_instance_id) REFERENCES era_equipment_instance(id);

-- Only the empty reward configuration envelope changes; there are no business entries to convert.
UPDATE era_reward_configuration SET payload = JSON_SET(payload, '$.SchemaVersion', 2), version = version + 1;
