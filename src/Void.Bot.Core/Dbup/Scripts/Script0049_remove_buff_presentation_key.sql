-- Effects use their stable Id to select client presentation. Keep identities and rules.
-- Historical publications remain immutable; their retired field is removed on read.
-- DbUp supplies the transaction. No stored routines or temporary tables are required.

WITH entries AS (
    SELECT configuration.id,
        CASE WHEN JSON_CONTAINS_PATH(configuration.payload, 'one', '$.Buffs')
            THEN '$.Buffs' ELSE '$.buffs' END AS buffs_path,
        item.item_index,
        JSON_EXTRACT(COALESCE(JSON_EXTRACT(configuration.payload, '$.Buffs'),
            JSON_EXTRACT(configuration.payload, '$.buffs')),
            CONCAT('$[', item.item_index - 1, ']')) AS definition
    FROM era_skill_configuration configuration
    JOIN JSON_TABLE(COALESCE(JSON_EXTRACT(configuration.payload, '$.Buffs'),
        JSON_EXTRACT(configuration.payload, '$.buffs'), JSON_ARRAY()),
        '$[*]' COLUMNS (item_index FOR ORDINALITY)) item ON TRUE
), cleaned AS (
    SELECT id, buffs_path, item_index,
        JSON_ARRAYAGG(JSON_REMOVE(definition, '$.PresentationKey', '$.presentationKey'))
            OVER (PARTITION BY id ORDER BY item_index
                ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) AS buffs,
        MAX(JSON_CONTAINS_PATH(definition, 'one', '$.PresentationKey', '$.presentationKey'))
            OVER (PARTITION BY id) AS changed
    FROM entries
)
UPDATE era_skill_configuration configuration
JOIN cleaned ON cleaned.id = configuration.id AND cleaned.item_index = 1 AND cleaned.changed = 1
SET configuration.payload = JSON_SET(configuration.payload, cleaned.buffs_path, CAST(cleaned.buffs AS JSON));

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
