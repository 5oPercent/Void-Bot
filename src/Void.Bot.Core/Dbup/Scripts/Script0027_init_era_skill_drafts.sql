CREATE TABLE `era_skill_draft_workspace` (
 `id` CHAR(36) NOT NULL, `initialized` TINYINT(1) NOT NULL DEFAULT 0,
 `revision` BIGINT NOT NULL DEFAULT 0, `base_published_version` BIGINT NOT NULL DEFAULT 0,
 `published_revision` BIGINT NOT NULL DEFAULT 0, `settings_version` BIGINT NOT NULL DEFAULT 0,
 `settings_payload` MEDIUMTEXT NOT NULL, `last_modified_by` CHAR(36) NULL, `updated_at` DATETIME(6) NULL,
 PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `era_skill_draft_workspace` (`id`, `settings_payload`)
VALUES ('20000000-0000-0000-0000-000000000002', '{"CommonTacticalSkills":[],"DisabledSkills":[]}');

CREATE TABLE `era_skill_draft_record` (
 `id` CHAR(36) NOT NULL, `workspace_id` CHAR(36) NOT NULL, `kind` INT NOT NULL,
 `stable_id` VARCHAR(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL,
 `version` BIGINT NOT NULL, `payload` MEDIUMTEXT NOT NULL, `is_deleted` TINYINT(1) NOT NULL,
 `was_published` TINYINT(1) NOT NULL, `last_modified_by` CHAR(36) NULL, `updated_at` DATETIME(6) NULL,
 PRIMARY KEY (`id`), UNIQUE KEY `ux_era_draft_kind_id` (`workspace_id`,`kind`,`stable_id`),
 CONSTRAINT `fk_era_draft_workspace` FOREIGN KEY (`workspace_id`) REFERENCES `era_skill_draft_workspace` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_skill_publication` (
 `id` CHAR(36) NOT NULL, `version` BIGINT NOT NULL, `draft_revision` BIGINT NOT NULL,
 `payload` MEDIUMTEXT NOT NULL, `content_hash` VARCHAR(64) NOT NULL,
 `published_by` CHAR(36) NULL, `published_at` DATETIME(6) NOT NULL,
 PRIMARY KEY (`id`), UNIQUE KEY `ux_era_skill_publication_version` (`version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `era_skill_publish_receipt` (
 `id` CHAR(36) NOT NULL, `account_id` CHAR(36) NOT NULL,
 `expected_draft_revision` BIGINT NOT NULL, `expected_published_version` BIGINT NOT NULL,
 `published_version` BIGINT NOT NULL, `created_at` DATETIME(6) NOT NULL,
 PRIMARY KEY (`id`), KEY `ix_era_skill_receipt_version` (`published_version`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
