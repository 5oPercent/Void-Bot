CREATE TABLE friend_request (
    id CHAR(36) NOT NULL,
    sender_account_id CHAR(36) NOT NULL,
    recipient_account_id CHAR(36) NOT NULL,
    request_id CHAR(36) NOT NULL,
    status INT NOT NULL,
    created_at DATETIME(6) NOT NULL,
    processed_at DATETIME(6) NULL,
    version BIGINT NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY ux_friend_request_receipt (sender_account_id, request_id),
    KEY ix_friend_request_recipient (recipient_account_id, status, created_at),
    KEY ix_friend_request_sender (sender_account_id, status, created_at),
    CONSTRAINT fk_friend_request_sender FOREIGN KEY (sender_account_id) REFERENCES user_account(id),
    CONSTRAINT fk_friend_request_recipient FOREIGN KEY (recipient_account_id) REFERENCES user_account(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE friendship (
    id CHAR(36) NOT NULL,
    first_account_id CHAR(36) NOT NULL,
    second_account_id CHAR(36) NOT NULL,
    created_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY ux_friendship_pair (first_account_id, second_account_id),
    KEY ix_friendship_second (second_account_id),
    CONSTRAINT fk_friendship_first FOREIGN KEY (first_account_id) REFERENCES user_account(id),
    CONSTRAINT fk_friendship_second FOREIGN KEY (second_account_id) REFERENCES user_account(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE account_block (
    id CHAR(36) NOT NULL,
    account_id CHAR(36) NOT NULL,
    blocked_account_id CHAR(36) NOT NULL,
    created_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY ux_account_block_pair (account_id, blocked_account_id),
    CONSTRAINT fk_account_block_owner FOREIGN KEY (account_id) REFERENCES user_account(id),
    CONSTRAINT fk_account_block_target FOREIGN KEY (blocked_account_id) REFERENCES user_account(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE social_notification_outbox (
    id CHAR(36) NOT NULL,
    recipient_account_id CHAR(36) NOT NULL,
    era_only TINYINT(1) NOT NULL,
    payload VARCHAR(4096) NOT NULL,
    attempts INT NOT NULL,
    next_attempt_at DATETIME(6) NOT NULL,
    completed_at DATETIME(6) NULL,
    created_at DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    KEY ix_social_outbox_due (completed_at, next_attempt_at),
    KEY ix_social_outbox_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
