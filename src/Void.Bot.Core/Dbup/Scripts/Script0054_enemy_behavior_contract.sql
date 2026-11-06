-- Runs under DbUp's transaction. No business table is created or dropped.
-- Stop all configuration writers and deploy the backend, Web and RPC 32 client together.
CREATE TEMPORARY TABLE era_enemy_behavior_check (name VARCHAR(64) PRIMARY KEY) ENGINE=InnoDB;
INSERT INTO era_enemy_behavior_check VALUES ('json'),('shape'),('skill_reference'),('concurrency'),('budget'),('version');
INSERT INTO era_enemy_behavior_check SELECT 'json' WHERE EXISTS
 (SELECT 1 FROM era_enemy_definition WHERE JSON_VALID(payload)=0);
INSERT INTO era_enemy_behavior_check SELECT 'shape' WHERE EXISTS
 (SELECT 1 FROM era_enemy_definition WHERE COALESCE(JSON_TYPE(JSON_EXTRACT(payload,'$.Ai')),'')<>'OBJECT'
 OR COALESCE(JSON_TYPE(JSON_EXTRACT(payload,'$.Skills')),'')<>'ARRAY' OR JSON_LENGTH(JSON_EXTRACT(payload,'$.Skills'))>256
 OR JSON_CONTAINS_PATH(payload,'one','$.ai','$.skills'));

CREATE TEMPORARY TABLE era_enemy_behavior_stage (
 id CHAR(36) PRIMARY KEY, old_version BIGINT NOT NULL, old_payload MEDIUMTEXT NOT NULL, payload JSON NOT NULL
) ENGINE=InnoDB;
INSERT INTO era_enemy_behavior_stage SELECT id,version,payload,CAST(payload AS JSON) FROM era_enemy_definition FOR UPDATE;
INSERT INTO era_enemy_behavior_check SELECT 'version' WHERE EXISTS
 (SELECT 1 FROM era_enemy_behavior_stage WHERE old_version=9223372036854775807)
 OR EXISTS (SELECT 1 FROM era_data_edit_state WHERE revision=9223372036854775807);

-- Legacy skills retain equal action weights and Priority as the tie-breaker. Disable jitter
-- for migrated records so the old ordering is not silently turned into a random order.
UPDATE era_enemy_behavior_stage SET payload=JSON_INSERT(payload,'$.Ai.ScoreJitter',0)
 WHERE JSON_CONTAINS_PATH(payload,'one','$.Ai.Behavior','$.Ai.MaximumChaseSeconds','$.Ai.IdleMinimumSeconds','$.Ai.IdleMaximumSeconds');
UPDATE era_enemy_behavior_stage SET payload=JSON_REMOVE(payload,
 '$.Ai.Behavior','$.Ai.MaximumChaseSeconds','$.Ai.IdleMinimumSeconds','$.Ai.IdleMaximumSeconds');

CREATE TEMPORARY TABLE era_enemy_behavior_index (n INT PRIMARY KEY);
INSERT INTO era_enemy_behavior_index
 SELECT a.n+b.n*10+c.n*100 FROM
 (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) a
 CROSS JOIN (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) b
 CROSS JOIN (SELECT 0 n UNION ALL SELECT 1 UNION ALL SELECT 2) c WHERE a.n+b.n*10+c.n*100<256;
CREATE TEMPORARY TABLE era_enemy_behavior_skill (
 enemy_id CHAR(36) NOT NULL, n INT NOT NULL, reference JSON NOT NULL, carrier INT NULL, PRIMARY KEY(enemy_id,n)
) ENGINE=InnoDB;
INSERT INTO era_enemy_behavior_skill (enemy_id,n,reference)
 SELECT e.id,i.n,JSON_EXTRACT(e.payload,CONCAT('$.Skills[',i.n,']'))
 FROM era_enemy_behavior_stage e JOIN era_enemy_behavior_index i ON i.n<JSON_LENGTH(JSON_EXTRACT(e.payload,'$.Skills'));
UPDATE era_enemy_behavior_skill e JOIN era_skill_definition s
 ON BINARY s.stable_id=BINARY JSON_UNQUOTE(JSON_EXTRACT(e.reference,'$.SkillId'))
 JOIN era_enemy_behavior_index i ON i.n<JSON_LENGTH(JSON_EXTRACT(s.payload,'$.Variants'))
 AND JSON_EXTRACT(s.payload,CONCAT('$.Variants[',i.n,'].Slot'))=JSON_EXTRACT(e.reference,'$.Slot')
 SET e.carrier=JSON_EXTRACT(s.payload,CONCAT('$.Variants[',i.n,'].CarrierKind'));
INSERT INTO era_enemy_behavior_check SELECT 'skill_reference' WHERE EXISTS
 (SELECT 1 FROM era_enemy_behavior_skill WHERE carrier IS NULL OR carrier NOT IN (0,1,2,3));
UPDATE era_enemy_behavior_skill SET reference=JSON_INSERT(reference,'$.Decision',JSON_OBJECT(
 'Target',IF(carrier=0,1,0),'MinimumDistance',0,'MaximumDistance',IF(carrier=0,0,30),
 'RequireLineOfSight',CAST(IF(carrier=0,'false','true') AS JSON),
 'MinimumHealthRatio',0,'MaximumHealthRatio',1,'MaximumTargetHealthRatio',1,'Weight',1,
 'InterruptOnTargetLost',CAST('true' AS JSON),'OncePerEngagement',CAST('false' AS JSON),'Token',IF(carrier=0,2,1)));
-- Explicit index order preserves the final tie-breaker. The sum of valid template payloads
-- is capped at 1 MiB by the catalog; increase this session-only aggregation limit accordingly.
SET @era_enemy_previous_concat=@@SESSION.group_concat_max_len;
SET SESSION group_concat_max_len=1048576;
UPDATE era_enemy_behavior_stage e JOIN
 (SELECT enemy_id,CONCAT('[',GROUP_CONCAT(CAST(reference AS CHAR) ORDER BY n SEPARATOR ','),']') skills
 FROM era_enemy_behavior_skill GROUP BY enemy_id) s ON s.enemy_id=e.id
 SET e.payload=JSON_SET(e.payload,'$.Skills',CAST(s.skills AS JSON));
SET SESSION group_concat_max_len=@era_enemy_previous_concat;

INSERT INTO era_enemy_behavior_check SELECT 'budget' WHERE EXISTS
 (SELECT 1 FROM era_enemy_behavior_stage WHERE OCTET_LENGTH(CAST(payload AS CHAR))>262144);
-- MySQL cannot reopen a temporary table within one statement. Keep row and count checks separate.
INSERT INTO era_enemy_behavior_check SELECT 'concurrency' WHERE EXISTS
 (SELECT 1 FROM era_enemy_behavior_stage s LEFT JOIN era_enemy_definition d ON d.id=s.id
 WHERE d.id IS NULL OR d.version<>s.old_version OR BINARY d.payload<>BINARY s.old_payload);
INSERT INTO era_enemy_behavior_check SELECT 'concurrency' WHERE
 (SELECT COUNT(*) FROM era_enemy_behavior_stage)<>(SELECT COUNT(*) FROM era_enemy_definition);
SET @era_enemy_changed=(SELECT COUNT(*) FROM era_enemy_behavior_stage WHERE CAST(old_payload AS JSON)<>payload);
UPDATE era_enemy_definition d JOIN era_enemy_behavior_stage s ON s.id=d.id
 SET d.payload=CAST(s.payload AS CHAR),d.version=d.version+1
 WHERE CAST(s.old_payload AS JSON)<>s.payload AND d.version=s.old_version AND BINARY d.payload=BINARY s.old_payload;
UPDATE era_data_edit_state SET revision=revision+1 WHERE @era_enemy_changed>0;
DROP TEMPORARY TABLE era_enemy_behavior_skill;
DROP TEMPORARY TABLE era_enemy_behavior_index;
DROP TEMPORARY TABLE era_enemy_behavior_stage;
DROP TEMPORARY TABLE era_enemy_behavior_check;
