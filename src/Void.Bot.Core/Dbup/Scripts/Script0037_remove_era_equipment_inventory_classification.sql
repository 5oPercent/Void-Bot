-- 装备实例只保存实际位置；穿戴兼容关系由模板定义，背包按角色统一计数。
ALTER TABLE era_equipment_instance
    DROP INDEX ix_era_equipment_character_inventory,
    DROP COLUMN inventory_slot,
    ADD INDEX ix_era_equipment_character_inventory (character_id, location);
