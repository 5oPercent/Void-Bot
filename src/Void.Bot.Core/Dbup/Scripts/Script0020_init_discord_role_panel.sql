CREATE TABLE IF NOT EXISTS `discord_role_panel`
(
    `id` CHAR(36) NOT NULL,
    `guild_id` BIGINT UNSIGNED NOT NULL,
    `channel_id` BIGINT UNSIGNED NOT NULL,
    `message_id` BIGINT UNSIGNED NULL,
    `title` NVARCHAR(256) NOT NULL,
    `content` NVARCHAR(4000) NOT NULL,
    `enabled` BIT NOT NULL DEFAULT 1,
    `created_at` DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    `updated_at` DATETIME(3) NULL,
    PRIMARY KEY (`id`),
    UNIQUE INDEX `IX_DISCORD_ROLE_PANEL_GUILD` (`guild_id`)
) charset=utf8mb4;

CREATE TABLE IF NOT EXISTS `discord_role_panel_item`
(
    `id` CHAR(36) NOT NULL,
    `panel_id` CHAR(36) NOT NULL,
    `role_id` BIGINT UNSIGNED NOT NULL,
    `emoji` NVARCHAR(64) NOT NULL,
    `label` NVARCHAR(80) NOT NULL,
    `style` NVARCHAR(16) NOT NULL DEFAULT 'Secondary',
    `mode` NVARCHAR(16) NOT NULL DEFAULT 'Toggle',
    `sort` INT NOT NULL DEFAULT 0,
    `enabled` BIT NOT NULL DEFAULT 1,
    `created_at` DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
    `updated_at` DATETIME(3) NULL,
    PRIMARY KEY (`id`),
    UNIQUE INDEX `IX_DISCORD_ROLE_PANEL_ITEM_EMOJI` (`panel_id`, `emoji`),
    UNIQUE INDEX `IX_DISCORD_ROLE_PANEL_ITEM_ROLE` (`panel_id`, `role_id`),
    CONSTRAINT `FK_DISCORD_ROLE_PANEL_ITEM_PANEL` FOREIGN KEY (`panel_id`) REFERENCES `discord_role_panel` (`id`) ON DELETE CASCADE
) charset=utf8mb4;
