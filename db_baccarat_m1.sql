
CREATE SCHEMA `db_baccarat_m1` DEFAULT CHARACTER SET utf8 ;

CREATE TABLE `db_baccarat_m1`.`tbl_bet_record` (
  `bet_uuid` varchar(64) NOT NULL,
  `server_code` varchar(45) NOT NULL,
  `game_code` varchar(45) NOT NULL,
  `round_number` int(11) NOT NULL DEFAULT '0',
  `client_id` varchar(45) DEFAULT NULL,
  `front_end` varchar(45) DEFAULT NULL,
  `bet_pool` int(11) NOT NULL,
  `bet_amount` decimal(19,4) NOT NULL DEFAULT '0.0000',
  `game_result` int(11) NOT NULL DEFAULT '0',
  `pay_amount` decimal(19,4) NOT NULL DEFAULT '0.0000',
  `bet_state` int(11) NOT NULL DEFAULT '0',
  `merchant_code` varchar(64) NOT NULL,
  `player_id` varchar(64) NOT NULL,
  `bet_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `update_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`bet_uuid`),
  KEY `IDX_SERVER_CODE` (`server_code`),
  KEY `IDX_GAME_CODE` (`game_code`),
  KEY `IDX_ROUND_NUMBER` (`round_number`),
  KEY `IDX_GAME_ROUND` (`server_code`,`game_code`,`round_number`),
  KEY `IDX_CLIENT_ID` (`client_id`),
  KEY `IDX_FRONT_END` (`front_end`),
  KEY `IDX_BET_POOL` (`bet_pool`),
  KEY `IDX_GAME_RESULT` (`game_result`),
  KEY `IDX_BET_STATE` (`bet_state`),
  KEY `IDX_MERCHANT_CODE` (`merchant_code`),
  KEY `IDX_PLAYER_ID` (`player_id`),
  KEY `IDX_BET_TIME` (`bet_time`),
  KEY `IDX_UPDATE_TIME` (`update_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

