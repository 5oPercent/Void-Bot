ALTER TABLE `user_account`
    ADD COLUMN `email` VARCHAR(320) NULL,
    ADD COLUMN `email_verified_at` DATETIME NULL,
    ADD COLUMN `token_version` INT NOT NULL DEFAULT 0;

CREATE UNIQUE INDEX `ux_user_account_email`
    ON `user_account` (`email`);
