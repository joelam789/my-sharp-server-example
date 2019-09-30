
CREATE SCHEMA `db_sample_m2` DEFAULT CHARACTER SET utf8 ;

CREATE TABLE `db_sample_m2`.`tbl_bet_record` (
  `bet_uuid` varchar(64) NOT NULL,
  `debit_uuid` varchar(64) DEFAULT NULL,
  `credit_uuid` varchar(64) DEFAULT NULL,
  `provider_code` varchar(45) DEFAULT NULL,
  `game_code` varchar(45) DEFAULT NULL,
  `game_type` int(11) NOT NULL DEFAULT '0',
  `round_id` varchar(45) NOT NULL,
  `merchant_code` varchar(64) NOT NULL,
  `player_id` varchar(64) NOT NULL,
  `bet_pool` int(11) NOT NULL,
  `bet_amount` decimal(19,4) NOT NULL DEFAULT '0.0000',
  `pay_amount` decimal(19,4) NOT NULL DEFAULT '0.0000',
  `player_cards` varchar(500) DEFAULT NULL,
  `banker_cards` varchar(500) DEFAULT NULL,
  `game_result` varchar(1000) NOT NULL DEFAULT '0',
  `settle_state` int(11) NOT NULL DEFAULT '0',
  `is_cancelled` int(11) NOT NULL DEFAULT '0',
  `bet_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `settle_time` datetime DEFAULT NULL,
  `cancel_time` datetime DEFAULT NULL,
  `update_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `remark` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`bet_uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `db_sample_m2`.`tbl_player_balance` (
  `merchant_code` varchar(64) NOT NULL,
  `player_id` varchar(64) NOT NULL,
  `player_balance` decimal(19,4) NOT NULL DEFAULT '0.0000',
  `update_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`player_id`,`merchant_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `db_sample_m2`.`tbl_trans_credit` (
  `credit_uuid` varchar(64) NOT NULL,
  `bet_uuid` varchar(64) NOT NULL,
  `provider_code` varchar(45) DEFAULT NULL,
  `game_code` varchar(45) DEFAULT NULL,
  `round_id` varchar(45) NOT NULL,
  `merchant_code` varchar(64) NOT NULL,
  `player_id` varchar(64) NOT NULL,
  `bet_pool` int(11) NOT NULL,
  `credit_amount` decimal(19,4) NOT NULL DEFAULT '0.0000',
  `credit_success` int(11) NOT NULL DEFAULT '0',
  `credit_state` int(11) NOT NULL DEFAULT '0',
  `is_cancelled` int(11) NOT NULL DEFAULT '0',
  `cancel_success` int(11) NOT NULL DEFAULT '0',
  `last_return_code` int(11) NOT NULL DEFAULT '0',
  `bet_settle_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `cancel_time` datetime DEFAULT NULL,
  `update_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `remark` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`credit_uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE `db_sample_m2`.`tbl_trans_debit` (
  `debit_uuid` varchar(64) NOT NULL,
  `bet_uuid` varchar(64) NOT NULL,
  `provider_code` varchar(45) DEFAULT NULL,
  `game_code` varchar(45) DEFAULT NULL,
  `round_id` varchar(45) NOT NULL,
  `merchant_code` varchar(64) NOT NULL,
  `player_id` varchar(64) NOT NULL,
  `bet_pool` int(11) NOT NULL,
  `debit_amount` decimal(19,4) NOT NULL DEFAULT '0.0000',
  `debit_success` int(11) NOT NULL DEFAULT '0',
  `debit_state` int(11) NOT NULL DEFAULT '0',
  `is_cancelled` int(11) NOT NULL DEFAULT '0',
  `cancel_success` int(11) NOT NULL DEFAULT '0',
  `last_return_code` int(11) NOT NULL DEFAULT '0',
  `bet_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `cancel_time` datetime DEFAULT NULL,
  `update_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `remark` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`debit_uuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

