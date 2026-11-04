-- Effects use their stable Id for presentation; only the retired field is removed.
-- Keep array order, identities, rules and numeric values. Avoid JSON_TABLE/window JSON
-- aggregation so the migration does not require MySQL 8.0.4/8.0.14 features.
CREATE TEMPORARY TABLE era_effect_cleanup_check (check_name VARCHAR(64) NOT NULL PRIMARY KEY) ENGINE=InnoDB;
INSERT INTO era_effect_cleanup_check VALUES ('source_json'),('array_shape'),('array_case_conflict'),('array_limit'),('result_integrity');
INSERT INTO era_effect_cleanup_check
SELECT 'source_json' WHERE EXISTS (SELECT 1 FROM era_skill_configuration WHERE JSON_VALID(payload)=0);
CREATE TEMPORARY TABLE era_effect_cleanup_array (
 id CHAR(36) NOT NULL PRIMARY KEY, array_path VARCHAR(16) NOT NULL, definitions JSON NOT NULL
) ENGINE=InnoDB;
INSERT INTO era_effect_cleanup_array
SELECT id,IF(JSON_CONTAINS_PATH(payload,'one','$.Buffs'),'$.Buffs','$.buffs'),
 COALESCE(NULLIF(JSON_EXTRACT(payload,'$.Buffs'),CAST('null' AS JSON)),NULLIF(JSON_EXTRACT(payload,'$.buffs'),CAST('null' AS JSON)),JSON_ARRAY())
FROM era_skill_configuration;
INSERT INTO era_effect_cleanup_check
SELECT 'array_shape' WHERE EXISTS (SELECT 1 FROM era_effect_cleanup_array WHERE JSON_TYPE(definitions)<>'ARRAY');
INSERT INTO era_effect_cleanup_check
SELECT 'array_case_conflict' WHERE EXISTS (SELECT 1 FROM era_skill_configuration
 WHERE JSON_CONTAINS_PATH(payload,'all','$.Buffs','$.buffs') AND JSON_EXTRACT(payload,'$.Buffs')<>JSON_EXTRACT(payload,'$.buffs'));
INSERT INTO era_effect_cleanup_check
SELECT 'array_limit' WHERE EXISTS (SELECT 1 FROM era_effect_cleanup_array WHERE JSON_LENGTH(definitions)>10000);
CREATE TEMPORARY TABLE era_effect_cleanup_index (position INT NOT NULL PRIMARY KEY) ENGINE=InnoDB;
INSERT INTO era_effect_cleanup_index
SELECT a.n + b.n*10 + c.n*100 + d.n*1000
FROM (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) a
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) b
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) c
CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) d;
CREATE TEMPORARY TABLE era_effect_cleanup_entry (
 id CHAR(36) NOT NULL, position INT NOT NULL, definition JSON NOT NULL, changed TINYINT(1) NOT NULL,
 PRIMARY KEY (id,position)
) ENGINE=InnoDB;
INSERT INTO era_effect_cleanup_entry
SELECT a.id,i.position,JSON_REMOVE(JSON_EXTRACT(a.definitions,CONCAT('$[',i.position,']')),'$.PresentationKey','$.presentationKey'),
 JSON_CONTAINS_PATH(JSON_EXTRACT(a.definitions,CONCAT('$[',i.position,']')),'one','$.PresentationKey','$.presentationKey')
FROM era_effect_cleanup_array a JOIN era_effect_cleanup_index i ON i.position<JSON_LENGTH(a.definitions);
SET @era_effect_cleanup_concat_limit=@@SESSION.group_concat_max_len;
SET SESSION group_concat_max_len=CAST(GREATEST(@era_effect_cleanup_concat_limit,16777216) AS UNSIGNED);
CREATE TEMPORARY TABLE era_effect_cleanup_result (
 id CHAR(36) NOT NULL PRIMARY KEY, definitions MEDIUMTEXT NOT NULL, changed TINYINT(1) NOT NULL,
 expected_count INT NOT NULL, expected_bytes BIGINT NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
INSERT INTO era_effect_cleanup_result
SELECT id,CONCAT('[',GROUP_CONCAT(CAST(definition AS CHAR CHARACTER SET utf8mb4) ORDER BY position SEPARATOR ','),']'),
 MAX(changed),COUNT(*),SUM(OCTET_LENGTH(CAST(definition AS CHAR CHARACTER SET utf8mb4)))+COUNT(*)-1+2
FROM era_effect_cleanup_entry GROUP BY id;
SET SESSION group_concat_max_len=@era_effect_cleanup_concat_limit;
-- GROUP_CONCAT is length-bounded: reject any truncation before updating source payloads.
INSERT INTO era_effect_cleanup_check
SELECT 'result_integrity' WHERE EXISTS (SELECT 1 FROM era_effect_cleanup_result
 WHERE JSON_VALID(definitions)=0 OR OCTET_LENGTH(definitions)<>expected_bytes);
INSERT INTO era_effect_cleanup_check
SELECT 'result_integrity' WHERE EXISTS (SELECT 1 FROM era_effect_cleanup_result
 WHERE JSON_LENGTH(definitions)<>expected_count);
UPDATE era_skill_configuration configuration
JOIN era_effect_cleanup_result cleaned ON cleaned.id=configuration.id AND cleaned.changed=1
JOIN era_effect_cleanup_array source ON source.id=configuration.id
SET configuration.payload=JSON_SET(configuration.payload,source.array_path,CAST(cleaned.definitions AS JSON));
DROP TEMPORARY TABLE era_effect_cleanup_result;
DROP TEMPORARY TABLE era_effect_cleanup_entry;
DROP TEMPORARY TABLE era_effect_cleanup_index;
DROP TEMPORARY TABLE era_effect_cleanup_array;
DROP TEMPORARY TABLE era_effect_cleanup_check;

-- Invalidate stale draft writers once per affected workspace, including deleted records.
UPDATE era_skill_draft_workspace workspace
SET workspace.revision = workspace.revision + 1
WHERE EXISTS (
    SELECT 1 FROM era_skill_draft_record record
    WHERE record.workspace_id = workspace.id AND record.kind = 2
        AND JSON_CONTAINS_PATH(record.payload, 'one', '$.PresentationKey', '$.presentationKey')
);

UPDATE era_skill_draft_record
SET payload = JSON_REMOVE(payload, '$.PresentationKey', '$.presentationKey'),
    version = version + 1
WHERE kind = 2
    AND JSON_CONTAINS_PATH(payload, 'one', '$.PresentationKey', '$.presentationKey');
