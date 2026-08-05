CREATE UNIQUE INDEX `ux_user_third_party_provider_external`
    ON `user_third_party` (`provider`, `third_party_user_id`);

CREATE UNIQUE INDEX `ux_user_third_party_user_provider`
    ON `user_third_party` (`user_id`, `provider`);
