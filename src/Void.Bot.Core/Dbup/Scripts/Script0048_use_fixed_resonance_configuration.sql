-- Fixed resonance configuration, schema 3. No stored function or DELIMITER is required.
-- Run this whole script in the target database. DbUp supplies the deployment transaction.
-- Business definitions and publication versions are preserved; no legacy skills are converted.

-- 1. Remove retired editor metadata from the current catalog only when it exists.
UPDATE era_skill_configuration
SET payload = JSON_REMOVE(payload,
    '$.EditorDefinitions', '$.editorDefinitions',
    '$.ParameterFields', '$.parameterFields',
    '$.Carriers', '$.carriers')
WHERE JSON_CONTAINS_PATH(payload, 'one',
    '$.EditorDefinitions', '$.editorDefinitions',
    '$.ParameterFields', '$.parameterFields',
    '$.Carriers', '$.carriers');

-- 2. Upgrade the empty catalog seeded by earlier migrations. Leave any nonempty legacy
--    Skills collection on its original schema; changing a version does not convert skills.
UPDATE era_skill_configuration
SET payload = JSON_SET(payload,
    CASE WHEN JSON_CONTAINS_PATH(payload, 'one', '$.schemaVersion')
        THEN '$.schemaVersion' ELSE '$.SchemaVersion' END,
    3)
WHERE CAST(COALESCE(
    JSON_UNQUOTE(JSON_EXTRACT(payload, '$.SchemaVersion')),
    JSON_UNQUOTE(JSON_EXTRACT(payload, '$.schemaVersion')),
    '1') AS UNSIGNED) < 3
    AND COALESCE(JSON_LENGTH(JSON_EXTRACT(payload, '$.Skills')), 0) = 0
    AND COALESCE(JSON_LENGTH(JSON_EXTRACT(payload, '$.skills')), 0) = 0;

-- 3. Remove retired workspace metadata. Only an actual change advances draft versions.
UPDATE era_skill_draft_workspace
SET settings_payload = JSON_REMOVE(settings_payload,
        '$.EditorDefinitions', '$.editorDefinitions',
        '$.ParameterFields', '$.parameterFields',
        '$.Carriers', '$.carriers'),
    settings_version = settings_version + 1,
    revision = revision + 1
WHERE JSON_CONTAINS_PATH(settings_payload, 'one',
    '$.EditorDefinitions', '$.editorDefinitions',
    '$.ParameterFields', '$.parameterFields',
    '$.Carriers', '$.carriers');

-- 4. Remove the retired parameter-template menus and their permission bindings.
DELETE pm FROM permission_menu pm INNER JOIN menu m ON m.id = pm.menu_id
WHERE TRIM(TRAILING '/' FROM m.path) IN ('/admin/era/parameter-templates', '/admin/era/parameter-definitions');
DELETE FROM menu
WHERE TRIM(TRAILING '/' FROM path) IN ('/admin/era/parameter-templates', '/admin/era/parameter-definitions');
