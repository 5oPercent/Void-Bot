-- Existing templates remain the same assets/configuration identities. No instance data conversion or guard procedure.
CREATE TABLE era_weapon_framework (
    id CHAR(36) NOT NULL PRIMARY KEY, name VARCHAR(128) NOT NULL, note VARCHAR(2048) NOT NULL,
    is_enabled TINYINT(1) NOT NULL, presentation_driver_key VARCHAR(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_framework_stat (
    framework_id CHAR(36) NOT NULL, stat INT NOT NULL, display_order INT NOT NULL,
    PRIMARY KEY (framework_id, stat),
    CONSTRAINT fk_era_weapon_framework_stat FOREIGN KEY (framework_id) REFERENCES era_weapon_framework(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_weapon_framework_capability (
    framework_id CHAR(36) NOT NULL, capability_kind INT NOT NULL, configuration_payload TEXT NOT NULL,
    PRIMARY KEY (framework_id, capability_kind),
    CONSTRAINT fk_era_weapon_framework_capability FOREIGN KEY (framework_id) REFERENCES era_weapon_framework(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE era_weapon_template_stat (
    template_id CHAR(36) NOT NULL, stat INT NOT NULL, `value` DOUBLE NOT NULL,
    effect_at_zero DOUBLE NULL, effect_at_hundred DOUBLE NULL,
    PRIMARY KEY (template_id, stat),
    CONSTRAINT fk_era_weapon_template_stat FOREIGN KEY (template_id) REFERENCES era_weapon_template(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

ALTER TABLE era_weapon_template ADD COLUMN framework_id CHAR(36) NULL;
-- Give each existing template its own framework, preserving potentially different behavior and distance settings.
UPDATE era_weapon_template SET framework_id = UUID();
INSERT INTO era_weapon_framework (id, name, note, is_enabled, presentation_driver_key)
SELECT framework_id, LEFT(CONCAT(name, ' 框架'), 128), '', 1, 'Firearm' FROM era_weapon_template;

INSERT INTO era_weapon_framework_stat (framework_id, stat, display_order)
SELECT framework_id, 7, 0 FROM era_weapon_template UNION ALL
SELECT framework_id, 8, 1 FROM era_weapon_template UNION ALL
SELECT framework_id, 5, 2 FROM era_weapon_template UNION ALL
SELECT framework_id, 11, 3 FROM era_weapon_template WHERE charge_time IS NOT NULL UNION ALL
SELECT framework_id, 10, 4 FROM era_weapon_template WHERE projectile_speed IS NOT NULL;

INSERT INTO era_weapon_framework_capability (framework_id, capability_kind, configuration_payload)
SELECT framework_id, 1, JSON_OBJECT('Kind', 1, 'Aim', JSON_OBJECT()) FROM era_weapon_template UNION ALL
SELECT framework_id, 2, JSON_OBJECT('Kind', 2, 'Recoil', JSON_OBJECT()) FROM era_weapon_template UNION ALL
SELECT framework_id, 3, JSON_OBJECT('Kind', 3, 'Reload', JSON_OBJECT(
    'Mode', CAST(JSON_UNQUOTE(JSON_EXTRACT(behavior_payload, '$.ReloadMode')) AS UNSIGNED),
    'BaseSeconds', CAST(JSON_UNQUOTE(JSON_EXTRACT(behavior_payload, '$.BaseReloadSeconds')) AS DECIMAL(24,12)))) FROM era_weapon_template UNION ALL
SELECT framework_id, 4, JSON_OBJECT('Kind', 4, 'Charge', JSON_OBJECT('AllowPartialRelease', JSON_EXTRACT('false', '$')))
    FROM era_weapon_template WHERE charge_time IS NOT NULL UNION ALL
SELECT framework_id, 5, JSON_OBJECT('Kind', 5, 'Projectile', JSON_OBJECT('MaximumDistance', `range`))
    FROM era_weapon_template WHERE JSON_EXTRACT(behavior_payload, '$.HitMode') = 2 UNION ALL
SELECT framework_id, 7, JSON_OBJECT('Kind', 7, 'Firing', JSON_OBJECT(
    'Mode', CAST(JSON_UNQUOTE(JSON_EXTRACT(behavior_payload, '$.FireMode')) AS UNSIGNED),
    'BurstCount', IF(JSON_EXTRACT(behavior_payload, '$.FireMode') = 3,
        CAST(JSON_UNQUOTE(JSON_EXTRACT(behavior_payload, '$.BurstCount')) AS UNSIGNED), 1))) FROM era_weapon_template UNION ALL
SELECT framework_id, 8, JSON_OBJECT('Kind', 8, 'Hitscan', JSON_OBJECT('MaximumDistance', `range`))
    FROM era_weapon_template WHERE JSON_EXTRACT(behavior_payload, '$.HitMode') = 1;

-- Copy all values and per-template score endpoints before dropping the source columns/table.
-- Handling only controls drawing this weapon; legacy aim-time endpoints are intentionally removed.
-- Penetration starts at zero; legacy Range is moved into the capability distance above.
INSERT INTO era_weapon_template_stat (template_id, stat, `value`, effect_at_zero, effect_at_hundred)
SELECT id, 1, damage, NULL, NULL FROM era_weapon_template UNION ALL
SELECT id, 2, frequency, NULL, NULL FROM era_weapon_template UNION ALL
SELECT id, 3, loaded_capacity, NULL, NULL FROM era_weapon_template UNION ALL
SELECT id, 4, reserve_capacity, NULL, NULL FROM era_weapon_template UNION ALL
SELECT id, 5, reload_speed, NULL, NULL FROM era_weapon_template UNION ALL
SELECT id, 6, 0, NULL, NULL FROM era_weapon_template UNION ALL
SELECT t.id, 7, t.accuracy, m.spread_degrees_at_zero, m.spread_degrees_at_hundred
    FROM era_weapon_template t JOIN era_weapon_stat_mapping m ON t.stat_mapping_id = m.id UNION ALL
SELECT t.id, 8, t.stability, m.recoil_degrees_at_zero, m.recoil_degrees_at_hundred
    FROM era_weapon_template t JOIN era_weapon_stat_mapping m ON t.stat_mapping_id = m.id UNION ALL
SELECT t.id, 9, t.handling, m.ready_seconds_at_zero, m.ready_seconds_at_hundred
    FROM era_weapon_template t JOIN era_weapon_stat_mapping m ON t.stat_mapping_id = m.id UNION ALL
SELECT id, 10, projectile_speed, NULL, NULL FROM era_weapon_template WHERE projectile_speed IS NOT NULL UNION ALL
SELECT id, 11, charge_time, NULL, NULL FROM era_weapon_template WHERE charge_time IS NOT NULL;

-- User-approved removal of obsolete Range modifiers; keep each modification, its other modifiers and pool references.
-- Remove legacy Range modifiers before stat 6 is used for Penetration in schema 3; do not convert their values.
DELETE FROM era_weapon_mod_stat_modifier WHERE stat = 6;

ALTER TABLE era_weapon_template
    DROP FOREIGN KEY fk_era_weapon_stat_mapping,
    DROP COLUMN stat_mapping_id,
    MODIFY COLUMN framework_id CHAR(36) NOT NULL,
    DROP COLUMN damage, DROP COLUMN frequency,
    DROP COLUMN loaded_capacity, DROP COLUMN reserve_capacity,
    DROP COLUMN reload_speed, DROP COLUMN accuracy, DROP COLUMN stability,
    DROP COLUMN handling, DROP COLUMN projectile_speed, DROP COLUMN charge_time,
    DROP COLUMN `range`, DROP COLUMN behavior_payload,
    ADD CONSTRAINT fk_era_weapon_template_framework FOREIGN KEY (framework_id) REFERENCES era_weapon_framework(id);
-- All surviving effects now belong to template attribute rows; no independent mapping identity remains.
DROP TABLE era_weapon_stat_mapping;

-- Only publication metadata is stored here; template and armor configuration rows are preserved.
UPDATE era_equipment_configuration SET payload = JSON_SET(payload, '$.SchemaVersion', 3), version = version + 1;
