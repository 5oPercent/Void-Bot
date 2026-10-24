-- 新武器表由 Script0035 直接使用 CHAR(36)。本脚本迁移原有护甲定义与共用装备引用。
-- 同一旧标识始终映射为同一 Guid；已有标准 Guid 只规范大小写，不重新生成实例或候选。
-- 临时函数只用于此次迁移，结束后删除；不修改名称、资源键、来源标识或奖励池标识。
DROP FUNCTION IF EXISTS era_migrate_equipment_definition_guid;
DROP FUNCTION IF EXISTS era_migrate_equipment_json_guids;

DELIMITER $$
CREATE FUNCTION era_migrate_equipment_definition_guid(legacy_id TEXT)
RETURNS CHAR(36) DETERMINISTIC NO SQL
BEGIN
    DECLARE digest CHAR(32);
    IF legacy_id IS NULL OR CHAR_LENGTH(TRIM(legacy_id)) = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Equipment definition ID cannot be empty during Guid migration';
    END IF;
    IF legacy_id REGEXP '^[[:xdigit:]]{8}-[[:xdigit:]]{4}-[[:xdigit:]]{4}-[[:xdigit:]]{4}-[[:xdigit:]]{12}$' THEN
        RETURN LOWER(legacy_id);
    END IF;
    SET digest = MD5(CONCAT('era.equipment.definition:', legacy_id));
    RETURN CONCAT(SUBSTRING(digest, 1, 8), '-', SUBSTRING(digest, 9, 4), '-3',
        SUBSTRING(digest, 14, 3), '-a', SUBSTRING(digest, 18, 3), '-', SUBSTRING(digest, 21, 12));
END$$

CREATE FUNCTION era_migrate_equipment_json_guids(payload LONGTEXT)
RETURNS LONGTEXT DETERMINISTIC NO SQL
BEGIN
    DECLARE result JSON;
    DECLARE pending JSON;
    DECLARE current_node JSON;
    DECLARE node_keys JSON;
    DECLARE current_path TEXT;
    DECLARE child_path TEXT;
    DECLARE field_name TEXT;
    DECLARE position INT;
    DECLARE item_count INT;
    IF payload IS NULL THEN RETURN NULL; END IF;
    IF NOT JSON_VALID(payload) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid equipment or reward JSON during Guid migration';
    END IF;
    SET result = CAST(payload AS JSON);
    SET pending = JSON_ARRAY('$');
    WHILE JSON_LENGTH(pending) > 0 DO
        SET position = JSON_LENGTH(pending) - 1;
        SET current_path = JSON_UNQUOTE(JSON_EXTRACT(pending, CONCAT('$[', position, ']')));
        SET pending = JSON_REMOVE(pending, CONCAT('$[', position, ']'));
        SET current_node = JSON_EXTRACT(result, current_path);
        IF JSON_TYPE(current_node) = 'OBJECT' THEN
            SET node_keys = JSON_KEYS(current_node);
            SET item_count = JSON_LENGTH(node_keys);
            SET position = 0;
            WHILE position < item_count DO
                SET field_name = JSON_UNQUOTE(JSON_EXTRACT(node_keys, CONCAT('$[', position, ']')));
                SET child_path = CONCAT(current_path, '.', JSON_QUOTE(field_name));
                IF field_name IN ('DefinitionId', 'TemplateId', 'ModDefinitionId') OR
                    (field_name = 'Id' AND (JSON_CONTAINS_PATH(current_node, 'one', '$.CompatibleSlots', '$.BaseStats') OR
                        JSON_CONTAINS_PATH(current_node, 'all', '$.Modifiers', '$.Slot'))) THEN
                    SET result = JSON_SET(result, child_path,
                        era_migrate_equipment_definition_guid(JSON_UNQUOTE(JSON_EXTRACT(result, child_path))));
                ELSEIF JSON_TYPE(JSON_EXTRACT(result, child_path)) IN ('OBJECT', 'ARRAY') THEN
                    SET pending = JSON_ARRAY_APPEND(pending, '$', child_path);
                END IF;
                SET position = position + 1;
            END WHILE;
        ELSEIF JSON_TYPE(current_node) = 'ARRAY' THEN
            SET position = 0;
            WHILE position < JSON_LENGTH(current_node) DO
                SET pending = JSON_ARRAY_APPEND(pending, '$', CONCAT(current_path, '[', position, ']'));
                SET position = position + 1;
            END WHILE;
        END IF;
    END WHILE;
    RETURN CAST(result AS CHAR CHARACTER SET utf8mb4);
END$$
DELIMITER ;

UPDATE era_equipment_configuration SET payload = era_migrate_equipment_json_guids(payload);
UPDATE era_reward_configuration SET payload = era_migrate_equipment_json_guids(payload);
UPDATE era_reward_event SET plan_payload = era_migrate_equipment_json_guids(plan_payload),
    result_payload = era_migrate_equipment_json_guids(result_payload);
UPDATE era_reward_grant SET result_payload = era_migrate_equipment_json_guids(result_payload);
UPDATE era_equipment_operation_receipt SET result_payload = era_migrate_equipment_json_guids(result_payload);
UPDATE era_equipment_instance SET definition_id = era_migrate_equipment_definition_guid(definition_id);

-- 沿用项目 Guid 的 CHAR(36) 存储约定，使用表默认比较规则。
ALTER TABLE era_equipment_instance MODIFY COLUMN definition_id CHAR(36) NOT NULL;

DROP FUNCTION era_migrate_equipment_json_guids;
DROP FUNCTION era_migrate_equipment_definition_guid;
