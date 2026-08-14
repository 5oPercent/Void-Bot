CREATE TABLE IF NOT EXISTS `security_bootstrap_claim` (
    `claim_key` VARCHAR(64) NOT NULL PRIMARY KEY,
    `user_id` CHAR(36) NOT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO `role` (
    `id`,
    `pid`,
    `name`,
    `description`,
    `code`,
    `sequence`,
    `status`,
    `built`,
    `created_at`,
    `created_by`
)
SELECT
    '10000000-0000-0000-0000-000000000000',
    '00000000-0000-0000-0000-000000000000',
    'SuperAdmin',
    'System super administrator',
    'SuperAdmin',
    0,
    1,
    1,
    CURRENT_TIMESTAMP,
    '00000000-0000-0000-0000-000000000000'
WHERE NOT EXISTS (
    SELECT 1
    FROM `role`
    WHERE `id` = '10000000-0000-0000-0000-000000000000'
);
