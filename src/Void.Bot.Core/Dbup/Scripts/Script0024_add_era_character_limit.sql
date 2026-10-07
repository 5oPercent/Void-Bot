-- 与现有配置保持同一版本；新增配置后统一递增以通知客户端刷新。
INSERT INTO `era_configuration` (`id`, `config_key`, `value_type`, `value`, `description`, `config_version`)
SELECT '10000000-0000-0000-0000-000000000011', 'character.active_limit', 'integer', '1',
       '每位玩家的可用角色上限（禁用角色不占用名额）', COALESCE(MAX(`config_version`), 1)
FROM `era_configuration`;

UPDATE `era_configuration`
SET `config_version` = `config_version` + 1;
