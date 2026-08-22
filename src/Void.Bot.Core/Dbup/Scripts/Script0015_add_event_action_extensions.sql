ALTER TABLE `event_action_rule`
    ADD COLUMN `conditions_json` JSON NULL AFTER `sequence`;

UPDATE `event_action_rule`
SET `conditions_json` = JSON_ARRAY()
WHERE `conditions_json` IS NULL;

ALTER TABLE `event_action_rule`
    MODIFY COLUMN `conditions_json` JSON NOT NULL;

CREATE TABLE IF NOT EXISTS `event_action_execution`
(
    `id` CHAR(36) NOT NULL,
    `event_id` CHAR(36) NOT NULL,
    `event` VARCHAR(128) NOT NULL,
    `event_occurred_at` DATETIME(3) NOT NULL,
    `rule_id` CHAR(36) NOT NULL,
    `rule_name` VARCHAR(128) NOT NULL,
    `action_index` INT NOT NULL,
    `action_type` VARCHAR(128) NOT NULL,
    `action_version` INT NOT NULL,
    `failure_policy` VARCHAR(32) NOT NULL,
    `status` VARCHAR(32) NOT NULL,
    `started_at` DATETIME(3) NOT NULL,
    `completed_at` DATETIME(3) NOT NULL,
    `duration_milliseconds` BIGINT NOT NULL,
    `error_message` VARCHAR(2000) NULL,
    PRIMARY KEY (`id`)
) charset=utf8mb4;

CREATE INDEX IX_EVENT_ACTION_EXECUTION_EVENT_STARTED_AT
    ON `event_action_execution` (`event`, `started_at`);

CREATE INDEX IX_EVENT_ACTION_EXECUTION_RULE_STARTED_AT
    ON `event_action_execution` (`rule_id`, `started_at`);

CREATE INDEX IX_EVENT_ACTION_EXECUTION_STATUS_STARTED_AT
    ON `event_action_execution` (`status`, `started_at`);
