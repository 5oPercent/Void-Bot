CREATE TABLE IF NOT EXISTS `event_action_rule`
(
    `id` CHAR(36) NOT NULL,
    `event` VARCHAR(128) NOT NULL,
    `enabled` BIT NOT NULL DEFAULT 1,
    `sequence` INT NOT NULL DEFAULT 0,
    `actions_json` JSON NOT NULL,
    `created_at` DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    `updated_at` DATETIME(3) NULL,
    `created_by` CHAR(36) NOT NULL,
    `last_modified_by` CHAR(36) NULL,
    PRIMARY KEY (`id`)
) charset=utf8mb4;

CREATE INDEX IX_EVENT_ACTION_RULE_EVENT_ENABLED_SEQUENCE
    ON `event_action_rule` (`event`, `enabled`, `sequence`);
