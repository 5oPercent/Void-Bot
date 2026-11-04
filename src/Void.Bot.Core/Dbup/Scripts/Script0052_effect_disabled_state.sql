-- Default to enabled only when no existing per-effect disable flag is present.
CREATE TEMPORARY TABLE era_effect_disable_check (check_name VARCHAR(64) NOT NULL PRIMARY KEY) ENGINE=InnoDB;
INSERT INTO era_effect_disable_check VALUES ('payload_flag'),('copied_flags');
INSERT INTO era_effect_disable_check
SELECT 'payload_flag' WHERE EXISTS (SELECT 1 FROM era_effect_definition
 WHERE JSON_CONTAINS_PATH(payload,'one','$.IsDisabled','$.isDisabled') AND (
 COALESCE(JSON_TYPE(COALESCE(JSON_EXTRACT(payload,'$.IsDisabled'),JSON_EXTRACT(payload,'$.isDisabled'))),'')<>'BOOLEAN'
 OR (JSON_CONTAINS_PATH(payload,'all','$.IsDisabled','$.isDisabled') AND JSON_EXTRACT(payload,'$.IsDisabled')<>JSON_EXTRACT(payload,'$.isDisabled'))));
CREATE TEMPORARY TABLE era_effect_disable_source (
 id CHAR(36) NOT NULL PRIMARY KEY, is_disabled TINYINT(1) NOT NULL, payload MEDIUMTEXT NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_effect_disable_source
SELECT id,COALESCE(JSON_UNQUOTE(COALESCE(JSON_EXTRACT(payload,'$.IsDisabled'),JSON_EXTRACT(payload,'$.isDisabled')))='true',0),
 IF(JSON_CONTAINS_PATH(payload,'one','$.IsDisabled','$.isDisabled'),JSON_REMOVE(payload,'$.IsDisabled','$.isDisabled'),payload)
FROM era_effect_definition;
ALTER TABLE era_effect_definition ADD COLUMN is_disabled TINYINT(1) NOT NULL DEFAULT 0;
UPDATE era_effect_definition effect JOIN era_effect_disable_source original ON original.id=effect.id
SET effect.is_disabled=original.is_disabled,effect.payload=original.payload;
INSERT INTO era_effect_disable_check
SELECT 'copied_flags' WHERE (SELECT COUNT(*) FROM era_effect_definition)<>(SELECT COUNT(*) FROM era_effect_disable_source);
INSERT INTO era_effect_disable_check
SELECT 'copied_flags' WHERE EXISTS (SELECT 1 FROM era_effect_disable_source original LEFT JOIN era_effect_definition effect ON effect.id=original.id
 WHERE effect.id IS NULL OR effect.is_disabled<>original.is_disabled OR BINARY effect.payload<>BINARY original.payload);
DROP TEMPORARY TABLE era_effect_disable_source;
DROP TEMPORARY TABLE era_effect_disable_check;
UPDATE era_data_edit_state SET revision=revision+1;
