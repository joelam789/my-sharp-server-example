
CREATE SCHEMA `db_mini_baccarat` DEFAULT CHARACTER SET utf8 ;

CREATE TABLE `db_mini_baccarat`.`tbl_bet_record` (
  `bet_id` int(11) NOT NULL AUTO_INCREMENT,
  `bet_uuid` varchar(64) NOT NULL,
  `client_id` varchar(45) NOT NULL,
  `front_end` varchar(45) NOT NULL,
  `server_code` varchar(45) NOT NULL,
  `game_code` varchar(45) NOT NULL,
  `round_number` int(11) NOT NULL,
  `merchant_code` varchar(64) NOT NULL,
  `player_id` varchar(64) NOT NULL,
  `bet_pool` int(11) NOT NULL DEFAULT '0',
  `bet_amount` decimal(19,4) DEFAULT '0.0000',
  `pay_amount` decimal(19,4) DEFAULT '0.0000',
  `bet_state` int(11) NOT NULL DEFAULT '0',
  `game_result` int(11) DEFAULT NULL,
  `bet_time` datetime DEFAULT NULL,
  `settle_time` datetime DEFAULT NULL,
  PRIMARY KEY (`bet_id`),
  KEY `IDX_GAME_ID` (`server_code`,`game_code`,`round_number`),
  KEY `IDX_FES_ID` (`front_end`),
  KEY `IDX_BET_STATE` (`bet_state`),
  KEY `IDX_BET_TIME` (`bet_time`),
  KEY `IDX_SETTLE_TIME` (`settle_time`),
  KEY `IDX_BET_UUID` (`bet_uuid`)
) ENGINE=ndbcluster AUTO_INCREMENT=89 DEFAULT CHARSET=utf8;

CREATE TABLE `db_mini_baccarat`.`tbl_round_state` (
  `server_code` varchar(45) NOT NULL,
  `game_code` varchar(45) NOT NULL,
  `round_number` int(11) NOT NULL,
  `round_state` int(11) NOT NULL,
  `round_state_text` varchar(45) DEFAULT NULL,
  `backup_number` int(11) NOT NULL DEFAULT '0',
  `bet_time_countdown` int(11) DEFAULT NULL,
  `player_cards` varchar(45) DEFAULT NULL,
  `banker_cards` varchar(45) DEFAULT NULL,
  `game_result` varchar(45) DEFAULT NULL,
  `game_history` varchar(1000) DEFAULT NULL,
  `round_start_time` datetime DEFAULT NULL,
  `round_update_time` datetime DEFAULT NULL,
  PRIMARY KEY (`server_code`,`game_code`,`round_number`)
) ENGINE=ndbcluster DEFAULT CHARSET=utf8;

