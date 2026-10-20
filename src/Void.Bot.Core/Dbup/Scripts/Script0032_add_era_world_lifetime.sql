-- 增量迁移，不改已存在的 Script0031；历史活动世界仍需经过宽限及两次在线检查。
ALTER TABLE `era_world`
    ADD COLUMN `last_active_at` DATETIME(6) NULL,
    ADD COLUMN `last_lifetime_check_at` DATETIME(6) NULL,
    ADD COLUMN `timeout_observed_at` DATETIME(6) NULL,
    ADD COLUMN `end_reason` INT NULL;

UPDATE `era_world` SET `last_active_at` = `created_at`;
ALTER TABLE `era_world`
    MODIFY COLUMN `last_active_at` DATETIME(6) NOT NULL,
    ADD KEY `ix_era_world_lifetime_check` (`state`, `last_lifetime_check_at`);
