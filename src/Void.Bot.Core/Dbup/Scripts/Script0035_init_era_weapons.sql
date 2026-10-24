-- 武器属性仅保存在模板；不生成正式配置、物品或实例属性副本。
CREATE TABLE era_weapon_stat_effect_profile (
    id CHAR(36) NOT NULL PRIMARY KEY, name VARCHAR(128) NOT NULL,
    spread_degrees_at_zero DOUBLE NOT NULL, spread_degrees_at_hundred DOUBLE NOT NULL,
    recoil_degrees_at_zero DOUBLE NOT NULL, recoil_degrees_at_hundred DOUBLE NOT NULL,
    ready_seconds_at_zero DOUBLE NOT NULL, ready_seconds_at_hundred DOUBLE NOT NULL,
    aim_seconds_at_zero DOUBLE NOT NULL, aim_seconds_at_hundred DOUBLE NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_template (
    id CHAR(36) NOT NULL PRIMARY KEY,
    name VARCHAR(128) NOT NULL, icon VARCHAR(256) NOT NULL, note VARCHAR(2048) NOT NULL,
    is_generation_enabled TINYINT(1) NOT NULL,
    presentation_key VARCHAR(128) NOT NULL,
    stat_effect_profile_id CHAR(36) NOT NULL,
    `damage` DOUBLE NOT NULL,
    `frequency` DOUBLE NOT NULL,
    `loaded_capacity` INT NOT NULL,
    `reserve_capacity` INT NOT NULL,
    `reload_speed` DOUBLE NOT NULL,
    `range` DOUBLE NOT NULL,
    `accuracy` DOUBLE NOT NULL,
    `stability` DOUBLE NOT NULL,
    `handling` DOUBLE NOT NULL,
    `projectile_speed` DOUBLE NULL,
    `charge_time` DOUBLE NULL,
    behavior_payload TEXT NOT NULL, compatible_slots_payload TEXT NOT NULL,
    CONSTRAINT fk_era_weapon_effect_profile FOREIGN KEY (stat_effect_profile_id) REFERENCES era_weapon_stat_effect_profile(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_mod_definition (
    id CHAR(36) NOT NULL PRIMARY KEY,
    name VARCHAR(128) NOT NULL, slot INT NOT NULL, description VARCHAR(2048) NOT NULL, icon VARCHAR(256) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_template_mod_slot (
    template_id CHAR(36) NOT NULL,
    slot INT NOT NULL, candidate_count INT NOT NULL, display_order INT NOT NULL,
    PRIMARY KEY (template_id, slot),
    CONSTRAINT fk_era_weapon_slot_template FOREIGN KEY (template_id) REFERENCES era_weapon_template(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_mod_stat_modifier (
    mod_definition_id CHAR(36) NOT NULL,
    `sequence` INT NOT NULL, stat INT NOT NULL, operation INT NOT NULL, `value` DOUBLE NOT NULL,
    PRIMARY KEY (mod_definition_id, `sequence`),
    CONSTRAINT fk_era_weapon_modifier_definition FOREIGN KEY (mod_definition_id) REFERENCES era_weapon_mod_definition(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_template_mod_pool (
    template_id CHAR(36) NOT NULL,
    slot INT NOT NULL,
    mod_definition_id CHAR(36) NOT NULL,
    weight INT NOT NULL,
    PRIMARY KEY (template_id, slot, mod_definition_id),
    CONSTRAINT fk_era_weapon_pool_slot FOREIGN KEY (template_id,slot) REFERENCES era_weapon_template_mod_slot(template_id,slot),
    CONSTRAINT fk_era_weapon_pool_mod FOREIGN KEY (mod_definition_id) REFERENCES era_weapon_mod_definition(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_mod_candidate (
    candidate_id CHAR(36) NOT NULL PRIMARY KEY, equipment_id CHAR(36) NOT NULL,
    slot INT NOT NULL,
    mod_definition_id CHAR(36) NOT NULL,
    display_order INT NOT NULL,
    UNIQUE KEY ux_era_weapon_candidate_identity (equipment_id,slot,candidate_id),
    UNIQUE KEY ux_era_weapon_candidate_definition (equipment_id,slot,mod_definition_id),
    CONSTRAINT fk_era_weapon_candidate_equipment FOREIGN KEY (equipment_id) REFERENCES era_equipment_instance(id),
    CONSTRAINT fk_era_weapon_candidate_mod FOREIGN KEY (mod_definition_id) REFERENCES era_weapon_mod_definition(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_mod_selection (
    equipment_id CHAR(36) NOT NULL, slot INT NOT NULL, selected_candidate_id CHAR(36) NOT NULL,
    PRIMARY KEY (equipment_id,slot),
    CONSTRAINT fk_era_weapon_selection_candidate FOREIGN KEY (equipment_id,slot,selected_candidate_id)
        REFERENCES era_weapon_mod_candidate(equipment_id,slot,candidate_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
