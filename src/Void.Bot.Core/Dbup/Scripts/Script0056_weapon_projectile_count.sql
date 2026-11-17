-- Existing weapon templates fire one projectile per shot by default.
ALTER TABLE era_weapon_template
    ADD COLUMN projectile_count INT NOT NULL DEFAULT 1;

UPDATE era_equipment_configuration SET version = version + 1;
