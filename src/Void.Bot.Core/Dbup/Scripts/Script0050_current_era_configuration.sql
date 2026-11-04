-- Preserve current business definitions before retiring the draft/publication tables.
-- An initialized workspace is the complete latest edit; explicit draft deletions stay deleted.
-- MySQL DDL implicitly commits: validate staged data BEFORE creating targets and verify every
-- copied row BEFORE dropping any source. DbUp's transaction does not make DDL rollback-safe.
CREATE TEMPORARY TABLE era_current_migration_check (
 check_name VARCHAR(64) NOT NULL PRIMARY KEY
) ENGINE=InnoDB;

INSERT INTO era_current_migration_check (check_name) VALUES
 ('source_rows'),
 ('source_json'),
 ('source_schema'),
 ('array_shape'),
 ('array_case_conflict'),
 ('array_limit'),
 ('draft_kind'),
 ('definition_identity'),
 ('definition_disabled_flag'),
 ('disabled_skill_references'),
 ('relations_shape'),
 ('disabled_skills_shape'),
 ('copied_lattices'),
 ('copied_skills'),
 ('copied_effects'),
 ('copied_enemies'),
 ('copied_settings'),
 ('copied_relations');

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'source_rows' WHERE (SELECT COUNT(*) FROM era_skill_configuration) <> 1
 OR (SELECT COUNT(*) FROM era_skill_configuration WHERE id='20000000-0000-0000-0000-000000000001') <> 1
 OR (SELECT COUNT(*) FROM era_skill_draft_workspace) <> 1
 OR (SELECT COUNT(*) FROM era_skill_draft_workspace WHERE id='20000000-0000-0000-0000-000000000002' AND initialized IN (0,1)) <> 1;

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'source_json' WHERE EXISTS (SELECT 1 FROM era_skill_configuration WHERE JSON_VALID(payload)=0)
 OR EXISTS (SELECT 1 FROM era_skill_draft_workspace WHERE JSON_VALID(settings_payload)=0)
 OR EXISTS (SELECT 1 FROM era_skill_draft_record WHERE is_deleted=0 AND JSON_VALID(payload)=0);

SET @era_current_published = (SELECT payload FROM era_skill_configuration WHERE id='20000000-0000-0000-0000-000000000001');
SET @era_current_settings = (SELECT settings_payload FROM era_skill_draft_workspace WHERE id='20000000-0000-0000-0000-000000000002');
SET @era_current_initialized = (SELECT initialized FROM era_skill_draft_workspace WHERE id='20000000-0000-0000-0000-000000000002');

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'source_schema' WHERE @era_current_initialized=0 AND COALESCE(JSON_UNQUOTE(COALESCE(JSON_EXTRACT(@era_current_published,'$.SchemaVersion'),JSON_EXTRACT(@era_current_published,'$.schemaVersion'))),'') <> '3';

CREATE TEMPORARY TABLE era_current_definition_array (
 kind INT NOT NULL PRIMARY KEY, definitions JSON NOT NULL
) ENGINE=InnoDB;
INSERT INTO era_current_definition_array (kind,definitions) VALUES

 (0,COALESCE(NULLIF(JSON_EXTRACT(@era_current_published,'$.Systems'),CAST('null' AS JSON)),NULLIF(JSON_EXTRACT(@era_current_published,'$.systems'),CAST('null' AS JSON)),JSON_ARRAY())),
 (1,COALESCE(NULLIF(JSON_EXTRACT(@era_current_published,'$.Skills'),CAST('null' AS JSON)),NULLIF(JSON_EXTRACT(@era_current_published,'$.skills'),CAST('null' AS JSON)),JSON_ARRAY())),
 (2,COALESCE(NULLIF(JSON_EXTRACT(@era_current_published,'$.Buffs'),CAST('null' AS JSON)),NULLIF(JSON_EXTRACT(@era_current_published,'$.buffs'),CAST('null' AS JSON)),JSON_ARRAY())),
 (3,COALESCE(NULLIF(JSON_EXTRACT(@era_current_published,'$.Enemies'),CAST('null' AS JSON)),NULLIF(JSON_EXTRACT(@era_current_published,'$.enemies'),CAST('null' AS JSON)),JSON_ARRAY()));

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'array_shape' WHERE @era_current_initialized=0 AND EXISTS (SELECT 1 FROM era_current_definition_array WHERE JSON_TYPE(definitions)<>'ARRAY');

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'array_case_conflict' WHERE @era_current_initialized=0 AND ((JSON_CONTAINS_PATH(@era_current_published,'all','$.Systems','$.systems') AND JSON_EXTRACT(@era_current_published,'$.Systems')<>JSON_EXTRACT(@era_current_published,'$.systems')) OR
 (JSON_CONTAINS_PATH(@era_current_published,'all','$.Skills','$.skills') AND JSON_EXTRACT(@era_current_published,'$.Skills')<>JSON_EXTRACT(@era_current_published,'$.skills')) OR
 (JSON_CONTAINS_PATH(@era_current_published,'all','$.Buffs','$.buffs') AND JSON_EXTRACT(@era_current_published,'$.Buffs')<>JSON_EXTRACT(@era_current_published,'$.buffs')) OR
 (JSON_CONTAINS_PATH(@era_current_published,'all','$.Enemies','$.enemies') AND JSON_EXTRACT(@era_current_published,'$.Enemies')<>JSON_EXTRACT(@era_current_published,'$.enemies')));

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'array_limit' WHERE @era_current_initialized=0 AND EXISTS (SELECT 1 FROM era_current_definition_array WHERE JSON_LENGTH(definitions)>10000);

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'draft_kind' WHERE EXISTS (SELECT 1 FROM era_skill_draft_record WHERE workspace_id<>'20000000-0000-0000-0000-000000000002'
 OR (@era_current_initialized=1 AND is_deleted=0 AND kind NOT IN (0,1,2,3)));

-- JSON arrays are expanded using an index table; this does not require JSON_TABLE().
CREATE TEMPORARY TABLE era_current_definition_index (position INT NOT NULL PRIMARY KEY);
INSERT INTO era_current_definition_index (position)

SELECT a.n + b.n*10 + c.n*100 + d.n*1000
FROM (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) a
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) b
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) c
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) d;

CREATE TEMPORARY TABLE era_current_definition_source (
 kind INT NOT NULL, id CHAR(36) NOT NULL,
 stable_id VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
 version BIGINT NOT NULL, payload MEDIUMTEXT NOT NULL,
 last_modified_by CHAR(36) NULL, updated_at DATETIME(6) NULL,
 PRIMARY KEY (kind,stable_id), UNIQUE KEY ux_era_current_source_id (id)
) ENGINE=InnoDB;
INSERT INTO era_current_definition_source (kind,id,stable_id,version,payload,last_modified_by,updated_at)
SELECT r.kind,r.id,r.stable_id,GREATEST(r.version,1),r.payload,r.last_modified_by,r.updated_at
FROM era_skill_draft_record r
WHERE @era_current_initialized=1 AND r.workspace_id='20000000-0000-0000-0000-000000000002' AND r.is_deleted=0;
INSERT INTO era_current_definition_source (kind,id,stable_id,version,payload,last_modified_by,updated_at)
SELECT a.kind,UUID(),JSON_UNQUOTE(JSON_EXTRACT(JSON_EXTRACT(a.definitions,CONCAT('$[',j.position,']')),'$.Id')),
 GREATEST(c.version,1),JSON_EXTRACT(a.definitions,CONCAT('$[',j.position,']')),c.last_modified_by,c.updated_at
FROM era_current_definition_array a
JOIN era_current_definition_index j ON j.position<JSON_LENGTH(a.definitions)
JOIN era_skill_configuration c ON c.id='20000000-0000-0000-0000-000000000001'
WHERE @era_current_initialized=0;

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'definition_identity' WHERE EXISTS (SELECT 1 FROM era_current_definition_source
 WHERE COALESCE(JSON_TYPE(JSON_EXTRACT(payload,'$.Id')),'')<>'STRING'
 OR COALESCE(JSON_UNQUOTE(JSON_EXTRACT(payload,'$.Id')),'')=''
 OR BINARY stable_id<>BINARY JSON_UNQUOTE(JSON_EXTRACT(payload,'$.Id'))
 OR CHAR_LENGTH(JSON_UNQUOTE(JSON_EXTRACT(payload,'$.Id')))>128
 OR BINARY stable_id<>BINARY TRIM(stable_id));

-- Settings absent from an older workspace must not erase published identities or flags.
-- Explicitly saved empty collections remain empty; absent/null settings fall back.
SET @era_current_relations = COALESCE(
 NULLIF(JSON_EXTRACT(IF(@era_current_initialized=1,@era_current_settings,@era_current_published),'$.CombatRelations'),CAST('null' AS JSON)),
 NULLIF(JSON_EXTRACT(IF(@era_current_initialized=1,@era_current_settings,@era_current_published),'$.combatRelations'),CAST('null' AS JSON)),
 NULLIF(JSON_EXTRACT(@era_current_published,'$.CombatRelations'),CAST('null' AS JSON)),
 NULLIF(JSON_EXTRACT(@era_current_published,'$.combatRelations'),CAST('null' AS JSON)),
 JSON_OBJECT('Species',JSON_ARRAY(),'Factions',JSON_ARRAY(),'SpeciesRules',JSON_ARRAY(),'FactionRules',JSON_ARRAY()));
SET @era_current_disabled_skills = COALESCE(
 NULLIF(JSON_EXTRACT(IF(@era_current_initialized=1,@era_current_settings,@era_current_published),'$.DisabledSkills'),CAST('null' AS JSON)),
 NULLIF(JSON_EXTRACT(IF(@era_current_initialized=1,@era_current_settings,@era_current_published),'$.disabledSkills'),CAST('null' AS JSON)),
 NULLIF(JSON_EXTRACT(@era_current_published,'$.DisabledSkills'),CAST('null' AS JSON)),
 NULLIF(JSON_EXTRACT(@era_current_published,'$.disabledSkills'),CAST('null' AS JSON)),JSON_ARRAY());
SET @era_current_ability_settings = JSON_OBJECT('DisabledSkills',CAST(@era_current_disabled_skills AS JSON));

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'relations_shape' WHERE COALESCE(JSON_TYPE(@era_current_relations),'')<>'OBJECT'
 OR COALESCE(JSON_TYPE(JSON_EXTRACT(@era_current_relations,'$.Species')),'')<>'ARRAY'
 OR COALESCE(JSON_TYPE(JSON_EXTRACT(@era_current_relations,'$.Factions')),'')<>'ARRAY'
 OR COALESCE(JSON_TYPE(JSON_EXTRACT(@era_current_relations,'$.SpeciesRules')),'')<>'ARRAY'
 OR COALESCE(JSON_TYPE(JSON_EXTRACT(@era_current_relations,'$.FactionRules')),'')<>'ARRAY';

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'disabled_skills_shape' WHERE COALESCE(JSON_TYPE(@era_current_disabled_skills),'')<>'ARRAY';

-- Validate the flags before any legacy table can be retired by the following scripts.
INSERT INTO era_current_migration_check (check_name)
SELECT 'definition_disabled_flag' WHERE EXISTS (SELECT 1 FROM era_current_definition_source
 WHERE kind IN (1,2) AND JSON_CONTAINS_PATH(payload,'one','$.IsDisabled','$.isDisabled') AND (
 COALESCE(JSON_TYPE(COALESCE(JSON_EXTRACT(payload,'$.IsDisabled'),JSON_EXTRACT(payload,'$.isDisabled'))),'')<>'BOOLEAN'
 OR (JSON_CONTAINS_PATH(payload,'all','$.IsDisabled','$.isDisabled') AND JSON_EXTRACT(payload,'$.IsDisabled')<>JSON_EXTRACT(payload,'$.isDisabled'))));
INSERT INTO era_current_migration_check (check_name)
SELECT 'disabled_skill_references' WHERE JSON_LENGTH(@era_current_disabled_skills)>10000 OR EXISTS (
 SELECT 1 FROM era_current_definition_index i LEFT JOIN era_current_definition_source s
 ON s.kind=1 AND BINARY s.stable_id=BINARY JSON_UNQUOTE(JSON_EXTRACT(@era_current_disabled_skills,CONCAT('$[',i.position,']')))
 WHERE i.position<JSON_LENGTH(@era_current_disabled_skills)
 AND (COALESCE(JSON_TYPE(JSON_EXTRACT(@era_current_disabled_skills,CONCAT('$[',i.position,']'))),'')<>'STRING' OR s.id IS NULL));

-- All source selection and shape checks have succeeded. Create the new business tables.
CREATE TABLE era_data_edit_state (
 id CHAR(36) NOT NULL, revision BIGINT NOT NULL DEFAULT 1, settings_version BIGINT NOT NULL DEFAULT 1,
 last_modified_by CHAR(36) NULL, updated_at DATETIME(6) NULL, PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_data_edit_state (id,revision,settings_version,last_modified_by,updated_at)
SELECT id,GREATEST(revision,1),GREATEST(settings_version,1),last_modified_by,updated_at
FROM era_skill_draft_workspace WHERE id='20000000-0000-0000-0000-000000000002';

CREATE TABLE era_lattice_definition (
 id CHAR(36) NOT NULL,
 stable_id VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
 version BIGINT NOT NULL, payload MEDIUMTEXT NOT NULL,
 last_modified_by CHAR(36) NULL, updated_at DATETIME(6) NULL,
 PRIMARY KEY (id), UNIQUE KEY ux_era_lattice_definition_key (stable_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_lattice_definition (id,stable_id,version,payload,last_modified_by,updated_at)
SELECT id,stable_id,version,payload,last_modified_by,updated_at FROM era_current_definition_source WHERE kind=0;

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_lattices' WHERE (SELECT COUNT(*) FROM era_lattice_definition)<>(SELECT COUNT(*) FROM era_current_definition_source WHERE kind=0);
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_lattices' WHERE EXISTS (SELECT 1 FROM era_current_definition_source s LEFT JOIN era_lattice_definition t ON BINARY t.stable_id=BINARY s.stable_id
 WHERE s.kind=0 AND (t.id IS NULL OR BINARY t.id<>BINARY s.id OR t.version<>s.version OR BINARY t.payload<>BINARY s.payload
 OR NOT(t.last_modified_by<=>s.last_modified_by) OR NOT(t.updated_at<=>s.updated_at)));

CREATE TABLE era_skill_definition (
 id CHAR(36) NOT NULL,
 stable_id VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
 version BIGINT NOT NULL, payload MEDIUMTEXT NOT NULL,
 last_modified_by CHAR(36) NULL, updated_at DATETIME(6) NULL,
 PRIMARY KEY (id), UNIQUE KEY ux_era_skill_definition_key (stable_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_skill_definition (id,stable_id,version,payload,last_modified_by,updated_at)
SELECT id,stable_id,version,payload,last_modified_by,updated_at FROM era_current_definition_source WHERE kind=1;

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_skills' WHERE (SELECT COUNT(*) FROM era_skill_definition)<>(SELECT COUNT(*) FROM era_current_definition_source WHERE kind=1);
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_skills' WHERE EXISTS (SELECT 1 FROM era_current_definition_source s LEFT JOIN era_skill_definition t ON BINARY t.stable_id=BINARY s.stable_id
 WHERE s.kind=1 AND (t.id IS NULL OR BINARY t.id<>BINARY s.id OR t.version<>s.version OR BINARY t.payload<>BINARY s.payload
 OR NOT(t.last_modified_by<=>s.last_modified_by) OR NOT(t.updated_at<=>s.updated_at)));

CREATE TABLE era_effect_definition (
 id CHAR(36) NOT NULL,
 stable_id VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
 version BIGINT NOT NULL, payload MEDIUMTEXT NOT NULL,
 last_modified_by CHAR(36) NULL, updated_at DATETIME(6) NULL,
 PRIMARY KEY (id), UNIQUE KEY ux_era_effect_definition_key (stable_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_effect_definition (id,stable_id,version,payload,last_modified_by,updated_at)
SELECT id,stable_id,version,payload,last_modified_by,updated_at FROM era_current_definition_source WHERE kind=2;

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_effects' WHERE (SELECT COUNT(*) FROM era_effect_definition)<>(SELECT COUNT(*) FROM era_current_definition_source WHERE kind=2);
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_effects' WHERE EXISTS (SELECT 1 FROM era_current_definition_source s LEFT JOIN era_effect_definition t ON BINARY t.stable_id=BINARY s.stable_id
 WHERE s.kind=2 AND (t.id IS NULL OR BINARY t.id<>BINARY s.id OR t.version<>s.version OR BINARY t.payload<>BINARY s.payload
 OR NOT(t.last_modified_by<=>s.last_modified_by) OR NOT(t.updated_at<=>s.updated_at)));

CREATE TABLE era_enemy_definition (
 id CHAR(36) NOT NULL,
 stable_id VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
 version BIGINT NOT NULL, payload MEDIUMTEXT NOT NULL,
 last_modified_by CHAR(36) NULL, updated_at DATETIME(6) NULL,
 PRIMARY KEY (id), UNIQUE KEY ux_era_enemy_definition_key (stable_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_enemy_definition (id,stable_id,version,payload,last_modified_by,updated_at)
SELECT id,stable_id,version,payload,last_modified_by,updated_at FROM era_current_definition_source WHERE kind=3;

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_enemies' WHERE (SELECT COUNT(*) FROM era_enemy_definition)<>(SELECT COUNT(*) FROM era_current_definition_source WHERE kind=3);
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_enemies' WHERE EXISTS (SELECT 1 FROM era_current_definition_source s LEFT JOIN era_enemy_definition t ON BINARY t.stable_id=BINARY s.stable_id
 WHERE s.kind=3 AND (t.id IS NULL OR BINARY t.id<>BINARY s.id OR t.version<>s.version OR BINARY t.payload<>BINARY s.payload
 OR NOT(t.last_modified_by<=>s.last_modified_by) OR NOT(t.updated_at<=>s.updated_at)));

CREATE TABLE era_ability_settings (
 id CHAR(36) NOT NULL, payload MEDIUMTEXT NOT NULL, PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE era_combat_relation_configuration (
 id CHAR(36) NOT NULL, payload MEDIUMTEXT NOT NULL, PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_ability_settings (id,payload)
VALUES ('20000000-0000-0000-0000-000000000002',@era_current_ability_settings);
INSERT INTO era_combat_relation_configuration (id,payload)
VALUES ('20000000-0000-0000-0000-000000000002',@era_current_relations);

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_settings' WHERE (SELECT COUNT(*) FROM era_ability_settings WHERE id='20000000-0000-0000-0000-000000000002' AND CAST(payload AS JSON)=CAST(@era_current_ability_settings AS JSON))<>1;

-- A duplicate check_name aborts the script instead of silently discarding data.
INSERT INTO era_current_migration_check (check_name)
SELECT 'copied_relations' WHERE (SELECT COUNT(*) FROM era_combat_relation_configuration WHERE id='20000000-0000-0000-0000-000000000002' AND CAST(payload AS JSON)=CAST(@era_current_relations AS JSON))<>1;

-- Weapon/armor definitions, slots, mods, instances and their metadata row are already
-- independent relational data. Do not update or delete any equipment table here.
-- Retire the old sources only after every selected definition and settings object matched.
DROP TABLE era_skill_publish_receipt;
DROP TABLE era_skill_publication;
DROP TABLE era_skill_draft_record;
DROP TABLE era_skill_draft_workspace;
DROP TABLE era_skill_configuration;

DROP TEMPORARY TABLE era_current_definition_source;
DROP TEMPORARY TABLE era_current_definition_array;
DROP TEMPORARY TABLE era_current_definition_index;
DROP TEMPORARY TABLE era_current_migration_check;
