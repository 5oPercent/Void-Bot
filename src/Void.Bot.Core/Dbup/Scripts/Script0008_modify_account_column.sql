ALTER TABLE `user_account`
    DROP COLUMN `level`,
    DROP COLUMN `source`;

ALTER TABLE `user_role`
    MODIFY COLUMN `deadline` DATETIME NULL;