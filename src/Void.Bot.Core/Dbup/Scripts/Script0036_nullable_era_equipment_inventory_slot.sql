-- 武器与护甲共用角色背包；武器以 equipment_type 标识，不配置虚拟背包槽位。
-- 保留护甲原有部位分类数据，应用层要求武器 inventory_slot 为空。
ALTER TABLE era_equipment_instance MODIFY COLUMN inventory_slot INT NULL;
