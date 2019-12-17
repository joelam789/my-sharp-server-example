
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

                    table.basicInfo.gameServer = item.server;
                    table.basicInfo.tableCode = item.table;
                    table.basicInfo.shoeCode = item.shoe;
                    table.basicInfo.roundNumber = parseInt(item.round, 10);
                    table.basicInfo.roundState = parseInt(item.state, 10);
                    table.basicInfo.roundStateText = item.status;
                    table.basicInfo.betTimeCountdown = parseInt(item.bet_countdown, 10);
                    table.basicInfo.nextRoundCountdown = parseInt(item.next_countdown, 10);
                    table.basicInfo.playerCards = item.player;
                    table.basicInfo.bankerCards = item.banker;
                    table.basicInfo.gameResult = item.result;
                    table.basicInfo.gameResultHistory = item.history;

                    table.updateSimpleRoadmap(item.history);

                    if (!isOldTableCode) {
                        messenger.gameState.baccaratTableStates.set(table.basicInfo.tableCode, table);
                        messenger.gameState.resetBaccaratState(table.basicInfo.tableCode);
                    }

                }

                for (let tableCode of clientTableCodes) {
                    if (serverTableCodes.indexOf(tableCode) < 0) {
                        messenger.gameState.baccaratTableStates.delete(tableCode);
                        messenger.gameState.baccaratStates.delete(tableCode);
                    }
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

export class BetResultHandler implements MessageHandler {
    handle(messenger: Messenger, msg: any): boolean {
        if (msg.msg == "bet_result") {
            messenger.isRequesting = false;
            let messages = [];
            for (let item of msg.results) {
                let uimsg = {
                    table: item.table,
                    shoe: item.shoe,
                    round: parseInt(item.round, 10),
                    pool: parseInt(item.pool, 10),
                    bet: parseFloat(item.bet),
                    payout: parseFloat(item.payout),
                    result: item.result
                }
                messages.push(uimsg);
            }
            messenger.dispatch(new UI.BetResultUpdate(messages));
            
            return true;
        }
        return false;
    }
}

