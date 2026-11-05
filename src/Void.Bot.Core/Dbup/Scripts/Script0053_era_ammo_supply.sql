-- Configuration only; ammunition in an active game remains host-authoritative.
ALTER TABLE era_weapon_framework
    ADD COLUMN default_ammo_supply INT NOT NULL DEFAULT 1;
ALTER TABLE era_weapon_template
    ADD COLUMN ammo_type INT NOT NULL DEFAULT 1,
    ADD COLUMN ammo_supply INT NOT NULL DEFAULT 1;

-- Preserve existing infinite reserve semantics; finite legacy templates use tactical ammunition.
UPDATE era_weapon_template t
JOIN era_weapon_template_stat s ON s.template_id = t.id AND s.stat = 4
SET t.ammo_type = CASE WHEN s.value = -1 THEN 0 ELSE 1 END;

-- Bring finite legacy capacity configurations into the new 100-round total budget.
UPDATE era_weapon_template_stat s
JOIN era_weapon_template t ON t.id = s.template_id
SET s.value = LEAST(s.value, 100)
WHERE t.ammo_type <> 0 AND s.stat = 3;
UPDATE era_weapon_template_stat reserve_stat
JOIN era_weapon_template t ON t.id = reserve_stat.template_id
JOIN era_weapon_template_stat loaded_stat ON loaded_stat.template_id = t.id AND loaded_stat.stat = 3
SET reserve_stat.value = LEAST(reserve_stat.value, 100 - loaded_stat.value)
WHERE t.ammo_type <> 0 AND reserve_stat.stat = 4;

UPDATE era_equipment_configuration SET payload = JSON_SET(payload, '$.SchemaVersion', 4), version = version + 1;
