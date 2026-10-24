RENAME TABLE era_weapon_stat_effect_profile TO era_weapon_stat_mapping;

ALTER TABLE era_weapon_template
    DROP FOREIGN KEY fk_era_weapon_effect_profile,
    CHANGE COLUMN stat_effect_profile_id stat_mapping_id CHAR(36) NOT NULL,
    ADD CONSTRAINT fk_era_weapon_stat_mapping FOREIGN KEY (stat_mapping_id) REFERENCES era_weapon_stat_mapping(id);
