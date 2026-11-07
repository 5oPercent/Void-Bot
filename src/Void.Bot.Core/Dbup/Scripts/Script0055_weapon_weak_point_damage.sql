-- Template values are independent. Selecting a framework copies its value in the editor.
-- Existing frameworks and templates keep their previous damage with a multiplier of 1.
ALTER TABLE era_weapon_framework
    ADD COLUMN weak_point_health_damage_multiplier DOUBLE NOT NULL DEFAULT 1,
    ADD COLUMN weak_point_protection_damage_multiplier DOUBLE NOT NULL DEFAULT 1;
ALTER TABLE era_weapon_template
    ADD COLUMN weak_point_health_damage_multiplier DOUBLE NOT NULL DEFAULT 1,
    ADD COLUMN weak_point_protection_damage_multiplier DOUBLE NOT NULL DEFAULT 1;

UPDATE era_equipment_configuration SET version = version + 1;
