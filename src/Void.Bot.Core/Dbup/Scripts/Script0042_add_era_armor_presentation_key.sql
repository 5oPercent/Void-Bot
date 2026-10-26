ALTER TABLE era_armor_template
    ADD COLUMN presentation_key VARCHAR(128) NOT NULL AFTER is_generation_enabled;
