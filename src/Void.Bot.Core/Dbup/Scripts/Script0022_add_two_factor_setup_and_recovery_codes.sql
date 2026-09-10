ALTER TABLE `user_account`
    ADD COLUMN `two_factor_setup_at` DATETIME NULL;

CREATE TABLE `user_account_two_factor_recovery_code` (
    `id` CHAR(36) NOT NULL,
    `user_id` CHAR(36) NOT NULL,
    `code_hash` VARCHAR(64) NOT NULL,
    `created_at` DATETIME(3) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE INDEX `ux_user_account_two_factor_recovery_code_hash` (`user_id`, `code_hash`),
    CONSTRAINT `fk_user_account_two_factor_recovery_code_user`
        FOREIGN KEY (`user_id`) REFERENCES `user_account` (`id`) ON DELETE CASCADE
);
