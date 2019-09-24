
CREATE SCHEMA `db_baccarat_main` DEFAULT CHARACTER SET utf8 ;

CREATE TABLE `db_baccarat_main`.`tbl_game_record` (
  `game_id` int(11) NOT NULL AUTO_INCREMENT,
  `server_code` varchar(45) NOT NULL,
  `table_code` varchar(45) NOT NULL,
  `shoe_code` varchar(45) NOT NULL,
  `round_number` int(11) NOT NULL DEFAULT '0',
  `round_state` int(11) NOT NULL DEFAULT '0',
  `player_cards` varchar(45) NOT NULL,
  `banker_cards` varchar(45) NOT NULL,
  `game_result` int(11) NOT NULL DEFAULT '0',
  `round_start_time` datetime DEFAULT NULL,
  `last_update_time` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`game_id`),
  UNIQUE KEY `UNI_GAME_ROUND` (`table_code`,`shoe_code`,`round_number`),
  KEY `IDX_SERVER_CODE` (`server_code`),
  KEY `IDX_ROUND_NUMBER` (`round_number`),
  KEY `IDX_ROUND_STATE` (`round_state`),
  KEY `IDX_GAME_RESULT` (`game_result`),
  KEY `IDX_START_TIME` (`round_start_time`),
  KEY `IDX_UPDATE_TIME` (`last_update_time`),
  KEY `IDX_SHOE_CODE` (`shoe_code`),
  KEY `IDX_TABLE_CODE` (`table_code`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8;

CREATE TABLE `db_baccarat_main`.`tbl_merchant_info` (
  `merchant_code` varchar(45) NOT NULL,
  `api_url` varchar(500) DEFAULT NULL,
  `is_active` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`merchant_code`),
  KEY `IDX_ACTIVE_STATE` (`is_active`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
