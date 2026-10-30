ALTER TABLE era_weapon_framework
    ADD COLUMN category VARCHAR(64) NOT NULL DEFAULT '未分类' AFTER name;

-- Existing driver keys cannot identify weapon categories such as auto rifles or pulse rifles.
-- Keep existing frameworks uncategorized until their category is explicitly configured.
