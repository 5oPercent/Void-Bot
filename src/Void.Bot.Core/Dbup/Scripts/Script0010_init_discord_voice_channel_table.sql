CREATE TABLE IF NOT EXISTS `discord_voice_channel_config`
(
    `id` CHAR(36) NOT NULL,
    `guild_id` BIGINT UNSIGNED NOT NULL,
    `trigger_channel_id` BIGINT UNSIGNED NOT NULL,
    `create_category_id` BIGINT UNSIGNED NOT NULL,
    `default_user_limit` INT NOT NULL DEFAULT 3,
    `channel_name_template` NVARCHAR(128) NOT NULL,
    `enabled` BIT NOT NULL DEFAULT 1,
    `created_at` DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    `updated_at` DATETIME(3) NULL,
    PRIMARY KEY (`id`)
) charset=utf8mb4;

CREATE UNIQUE INDEX IX_DISCORD_VOICE_CHANNEL_CONFIG_GUILD_TRIGGER
    ON `discord_voice_channel_config` (`guild_id`, `trigger_channel_id`);

CREATE INDEX IX_DISCORD_VOICE_CHANNEL_CONFIG_GUILD
    ON `discord_voice_channel_config` (`guild_id`);

CREATE TABLE IF NOT EXISTS `discord_temporary_voice_channel`
(
    `id` CHAR(36) NOT NULL,
    `guild_id` BIGINT UNSIGNED NOT NULL,
    `config_id` CHAR(36) NOT NULL,
    `trigger_channel_id` BIGINT UNSIGNED NOT NULL,
    `create_category_id` BIGINT UNSIGNED NOT NULL,
    `channel_id` BIGINT UNSIGNED NOT NULL,
    `owner_user_id` BIGINT UNSIGNED NOT NULL,
    `owner_display_name` NVARCHAR(128) NOT NULL,
    `user_limit` INT NOT NULL,
    `is_private` BIT NOT NULL DEFAULT 1,
    `last_empty_at` DATETIME(3) NULL,
    `deleted_at` DATETIME(3) NULL,
    `created_at` DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    `updated_at` DATETIME(3) NULL,
    PRIMARY KEY (`id`)
) charset=utf8mb4;

CREATE UNIQUE INDEX IX_DISCORD_TEMPORARY_VOICE_CHANNEL_CHANNEL
    ON `discord_temporary_voice_channel` (`channel_id`);

CREATE INDEX IX_DISCORD_TEMPORARY_VOICE_CHANNEL_GUILD_OWNER
    ON `discord_temporary_voice_channel` (`guild_id`, `owner_user_id`, `deleted_at`);

CREATE INDEX IX_DISCORD_TEMPORARY_VOICE_CHANNEL_EMPTY
    ON `discord_temporary_voice_channel` (`deleted_at`, `last_empty_at`);

CREATE TABLE IF NOT EXISTS `discord_temporary_voice_channel_allowed_user`
(
    `id` CHAR(36) NOT NULL,
    `temporary_voice_channel_id` CHAR(36) NOT NULL,
    `guild_id` BIGINT UNSIGNED NOT NULL,
    `channel_id` BIGINT UNSIGNED NOT NULL,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `created_at` DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    PRIMARY KEY (`id`)
) charset=utf8mb4;

CREATE UNIQUE INDEX IX_DISCORD_TEMPORARY_VOICE_CHANNEL_ALLOWED_USER_UNIQUE
    ON `discord_temporary_voice_channel_allowed_user` (`temporary_voice_channel_id`, `user_id`);

CREATE INDEX IX_DISCORD_TEMPORARY_VOICE_CHANNEL_ALLOWED_USER_CHANNEL
    ON `discord_temporary_voice_channel_allowed_user` (`channel_id`);
