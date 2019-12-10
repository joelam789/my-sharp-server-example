
CREATE SCHEMA `db_mini_baccarat` DEFAULT CHARACTER SET utf8 ;

CREATE TABLE `db_mini_baccarat`.`tbl_bet_record` (
  `bet_id` int(11) NOT NULL AUTO_INCREMENT,
  `bet_uuid` varchar(64) NOT NULL,
  `client_id` varchar(45) NOT NULL,
  `front_end` varchar(45) NOT NULL,
  `server_code` varchar(45) NOT NULL,
  `table_code` varchar(45) NOT NULL,
  `shoe_code` varchar(45) NOT NULL,
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
  UNIQUE KEY `UNI_BET_UUID` (`bet_uuid`),
  KEY `IDX_FES_ID` (`front_end`),
  KEY `IDX_BET_STATE` (`bet_state`),
  KEY `IDX_BET_TIME` (`bet_time`),
  KEY `IDX_SETTLE_TIME` (`settle_time`),
  KEY `IDX_SERVER_CODE` (`server_code`),
  KEY `IDX_TABLE_CODE` (`table_code`),
  KEY `IDX_GAME_ROUND` (`table_code`,`shoe_code`,`round_number`)
) ENGINE=ndbcluster AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;

CREATE TABLE `db_mini_baccarat`.`tbl_player_session` (
  `session_id` varchar(64) NOT NULL,
  `merchant_code` varchar(64) DEFAULT NULL,
  `player_id` varchar(64) DEFAULT NULL,
  `update_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`session_id`),
  KEY `IDX_PLAYER_ID` (`player_id`),
  KEY `IDX_MERCHANT` (`merchant_code`),
  KEY `IDX_UPDATE_TIME` (`update_time`)
) ENGINE=ndbcluster DEFAULT CHARSET=utf8;


CREATE TABLE `db_mini_baccarat`.`tbl_round_state` (
  `state_id` int(11) NOT NULL AUTO_INCREMENT,
  `server_code` varchar(45) NOT NULL,
  `table_code` varchar(45) NOT NULL,
  `shoe_code` varchar(45) NOT NULL,
  `round_number` int(11) NOT NULL,
  `round_state` int(11) NOT NULL,
  `round_state_text` varchar(45) DEFAULT NULL,
  `init_flag` int(11) DEFAULT '0',
  `backup_number` int(11) NOT NULL DEFAULT '0',
  `bet_time_countdown` int(11) DEFAULT NULL,
  `player_cards` varchar(45) DEFAULT NULL,
  `banker_cards` varchar(45) DEFAULT NULL,
  `game_result` varchar(45) DEFAULT NULL,
  `game_history` varchar(1000) DEFAULT NULL,
  `round_start_time` datetime DEFAULT NULL,
  `round_update_time` datetime DEFAULT NULL,
  PRIMARY KEY (`state_id`),
  UNIQUE KEY `UNI_GAME_ROUND` (`table_code`,`shoe_code`,`round_number`),
  UNIQUE KEY `UNI_GAME_HOST` (`table_code`,`init_flag`),
  KEY `IDX_SERVER_CODE` (`server_code`),
  KEY `IDX_TABLE_CODE` (`table_code`),
  KEY `IDX_ROUND_STATE` (`round_state`),
  KEY `IDX_BACKUP_NUMBER` (`backup_number`)
) ENGINE=ndbcluster AUTO_INCREMENT=1 DEFAULT CHARSET=utf8;

CREATE TABLE `db_mini_baccarat`.`tbl_bo_session` (
  `session_id` varchar(64) NOT NULL,
  `account_id` varchar(64) NOT NULL,
  `merchant_code` varchar(50) NOT NULL,
  `last_access_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`session_id`)
) ENGINE=ndbcluster DEFAULT CHARSET=utf8 COMMENT='backoffice sessions';




