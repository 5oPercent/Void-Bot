-- Preserve the current disable choices in the skill's own column before retiring settings.
CREATE TEMPORARY TABLE era_skill_disable_check (check_name VARCHAR(64) NOT NULL PRIMARY KEY) ENGINE=InnoDB;
INSERT INTO era_skill_disable_check VALUES ('settings_row'),('disabled_array'),('disabled_references'),('payload_flag'),('copied_flags');
INSERT INTO era_skill_disable_check
SELECT 'settings_row' WHERE (SELECT COUNT(*) FROM era_ability_settings)<>1
 OR (SELECT COUNT(*) FROM era_ability_settings WHERE id='20000000-0000-0000-0000-000000000002')<>1;
SET @era_disabled_skills = (SELECT COALESCE(JSON_EXTRACT(payload,'$.DisabledSkills'),JSON_ARRAY())
 FROM era_ability_settings WHERE id='20000000-0000-0000-0000-000000000002');
INSERT INTO era_skill_disable_check
SELECT 'disabled_array' WHERE COALESCE(JSON_TYPE(@era_disabled_skills),'')<>'ARRAY' OR JSON_LENGTH(@era_disabled_skills)>10000;
CREATE TEMPORARY TABLE era_skill_disable_index (position INT NOT NULL PRIMARY KEY) ENGINE=InnoDB;
INSERT INTO era_skill_disable_index (position)
SELECT a.n + b.n*10 + c.n*100 + d.n*1000
FROM (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) a
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) b
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) c
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) d;
INSERT INTO era_skill_disable_check
SELECT 'disabled_references' WHERE EXISTS (
 SELECT 1 FROM era_skill_disable_index i LEFT JOIN era_skill_definition s
 ON BINARY s.stable_id=BINARY JSON_UNQUOTE(JSON_EXTRACT(@era_disabled_skills,CONCAT('$[',i.position,']')))
 WHERE i.position<JSON_LENGTH(@era_disabled_skills)
 AND (COALESCE(JSON_TYPE(JSON_EXTRACT(@era_disabled_skills,CONCAT('$[',i.position,']'))),'')<>'STRING' OR s.id IS NULL));
INSERT INTO era_skill_disable_check
SELECT 'payload_flag' WHERE EXISTS (SELECT 1 FROM era_skill_definition
 WHERE JSON_CONTAINS_PATH(payload,'one','$.IsDisabled','$.isDisabled') AND (
 COALESCE(JSON_TYPE(COALESCE(JSON_EXTRACT(payload,'$.IsDisabled'),JSON_EXTRACT(payload,'$.isDisabled'))),'')<>'BOOLEAN'
 OR (JSON_CONTAINS_PATH(payload,'all','$.IsDisabled','$.isDisabled') AND JSON_EXTRACT(payload,'$.IsDisabled')<>JSON_EXTRACT(payload,'$.isDisabled'))));
CREATE TEMPORARY TABLE era_skill_disable_source (
 id CHAR(36) NOT NULL PRIMARY KEY, is_disabled TINYINT(1) NOT NULL, payload MEDIUMTEXT NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_skill_disable_source
SELECT id,CASE WHEN JSON_CONTAINS_PATH(payload,'one','$.IsDisabled','$.isDisabled')
 THEN JSON_UNQUOTE(COALESCE(JSON_EXTRACT(payload,'$.IsDisabled'),JSON_EXTRACT(payload,'$.isDisabled')))='true'
 ELSE JSON_CONTAINS(@era_disabled_skills,JSON_QUOTE(stable_id)) END,
 IF(JSON_CONTAINS_PATH(payload,'one','$.IsDisabled','$.isDisabled'),JSON_REMOVE(payload,'$.IsDisabled','$.isDisabled'),payload)
FROM era_skill_definition;
ALTER TABLE era_skill_definition ADD COLUMN is_disabled TINYINT(1) NOT NULL DEFAULT 0;
UPDATE era_skill_definition s JOIN era_skill_disable_source original ON original.id=s.id
SET s.is_disabled=original.is_disabled,s.payload=original.payload;
INSERT INTO era_skill_disable_check
SELECT 'copied_flags' WHERE (SELECT COUNT(*) FROM era_skill_definition)<>(SELECT COUNT(*) FROM era_skill_disable_source);
INSERT INTO era_skill_disable_check
SELECT 'copied_flags' WHERE EXISTS (SELECT 1 FROM era_skill_disable_source original LEFT JOIN era_skill_definition s ON s.id=original.id
 WHERE s.id IS NULL OR s.is_disabled<>original.is_disabled OR BINARY s.payload<>BINARY original.payload);
DROP TABLE era_ability_settings;
DROP TEMPORARY TABLE era_skill_disable_source;
DROP TEMPORARY TABLE era_skill_disable_index;
DROP TEMPORARY TABLE era_skill_disable_check;

UPDATE era_data_edit_state SET revision=revision+1,settings_version=settings_version+1;
DELETE pm FROM permission_menu pm INNER JOIN menu m ON m.id=pm.menu_id
WHERE TRIM(TRAILING '/' FROM m.path) IN ('/admin/era/ability-settings','/admin/era/ability-configuration','/admin/era/version-management');
DELETE FROM menu
WHERE TRIM(TRAILING '/' FROM path) IN ('/admin/era/ability-settings','/admin/era/ability-configuration','/admin/era/version-management');
