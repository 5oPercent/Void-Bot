ALTER TABLE `era_player_skill_profile`
    DROP FOREIGN KEY `fk_era_player_skill_profile_player`,
    DROP INDEX `ux_era_player_skill_profile_player`,
    CHANGE COLUMN `player_id` `character_id` CHAR(36) NOT NULL,
    ADD UNIQUE KEY `ux_era_character_skill_profile_character` (`character_id`),
    ADD CONSTRAINT `fk_era_character_skill_profile_character` FOREIGN KEY (`character_id`) REFERENCES `era_character` (`id`);

ALTER TABLE `era_player_learned_skill`
    DROP FOREIGN KEY `fk_era_player_learned_skill_player`,
    DROP INDEX `ux_era_player_learned_skill`,
    CHANGE COLUMN `player_id` `character_id` CHAR(36) NOT NULL,
    ADD UNIQUE KEY `ux_era_character_learned_skill` (`character_id`, `skill_id`),
    ADD CONSTRAINT `fk_era_character_learned_skill_character` FOREIGN KEY (`character_id`) REFERENCES `era_character` (`id`);

RENAME TABLE `era_player_skill_profile` TO `era_character_skill_profile`,
    `era_player_learned_skill` TO `era_character_learned_skill`;
