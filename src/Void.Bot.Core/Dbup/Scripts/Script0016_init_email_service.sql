CREATE TABLE IF NOT EXISTS `email_credential`
(
    `id` CHAR(36) NOT NULL,
    `sender_email` VARCHAR(320) NOT NULL,
    `external_account_id` VARCHAR(255) NOT NULL,
    `encrypted_access_token` TEXT NOT NULL,
    `access_token_expires_at` DATETIME(6) NOT NULL,
    `encrypted_refresh_token` TEXT NOT NULL,
    `refresh_token_last_used_at` DATETIME(6) NOT NULL,
    `next_refresh_at` DATETIME(6) NOT NULL,
    `status` INT NOT NULL,
    `last_error` VARCHAR(512) NULL,
    `last_error_at` DATETIME(6) NULL,
    `created_at` DATETIME(6) NOT NULL,
    `update_at` DATETIME(6) NULL,
    PRIMARY KEY (`id`)
) CHARSET=utf8mb4;
