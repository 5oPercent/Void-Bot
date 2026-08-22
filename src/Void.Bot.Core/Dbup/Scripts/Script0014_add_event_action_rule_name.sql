ALTER TABLE `event_action_rule`
    ADD COLUMN `name` VARCHAR(128) NOT NULL DEFAULT 'Unnamed rule' AFTER `id`;
