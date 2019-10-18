
import {BaccaratTableState} from './game-state';
import {Messenger} from './messenger';
import * as UI from './ui-messages';

export interface MessageHandler {
    handle(messenger: Messenger, msg: any): boolean;
}

export class TableInfoHandler implements MessageHandler {
    handle(messenger: Messenger, msg: any): boolean {
        if (msg.msg == "table_info") {
            messenger.isRequesting = false;
            let serverTableCodes = [];
            let clientTableCodes = Array.from(messenger.gameState.baccaratTableStates.keys());
            if (msg.tables) {

                for (let item of msg.tables) {

                    serverTableCodes.push(item.table);

                    let isOldTableCode = messenger.gameState.baccaratTableStates.has(item.table);
                    let table = isOldTableCode ?  messenger.gameState.baccaratTableStates.get(item.table) : new BaccaratTableState();

                    table.gameServer = item.server;
                    table.tableCode = item.table;
                    table.shoeCode = item.shoe;
                    table.roundNumber = parseInt(item.round, 10);
                    table.roundState = parseInt(item.state, 10);
                    table.roundStateText = item.status;
                    table.betTimeCountdown = parseInt(item.countdown, 10);
                    table.playerCards = item.player;
                    table.bankerCards = item.banker;
                    table.gameResult = item.result;
                    table.gameResultHistory = item.history;

                    table.updateSimpleRoadmap(item.history);

                    if (!isOldTableCode) messenger.gameState.baccaratTableStates.set(table.tableCode, table);

                }

                for (let tableCode of clientTableCodes) {
                    if (serverTableCodes.indexOf(tableCode) < 0) messenger.gameState.baccaratTableStates.delete(tableCode);
                }
                
                messenger.dispatch(new UI.TableInfoUpdate(msg.tables.length));
            }
            return true;
        }
        return false;
    }
}

export class ClientInfoHandler implements MessageHandler {
    handle(messenger: Messenger, msg: any): boolean {
        if (msg.msg == "client_info") {
            messenger.isRequesting = false;
            messenger.gameState.frontEndServerName = msg.front_end;
            messenger.gameState.frontEndClientId = msg.client_id;
            messenger.dispatch(new UI.ClientInfoUpdate(msg.front_end));
            return true;
        }
        return false;
    }
}

