ALTER TABLE `user_account`
    ADD COLUMN `two_factor_secret` VARCHAR(255) NULL,
    ADD COLUMN `two_factor_enabled_at` DATETIME NULL;
