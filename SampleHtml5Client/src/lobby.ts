
import {autoinject, customElement} from 'aurelia-framework';
import {EventAggregator, Subscription} from 'aurelia-event-aggregator';
import {Router} from 'aurelia-router';

import {DialogService} from 'aurelia-dialog';
import {I18N} from 'aurelia-i18n';

import {GameState, TableState, BetLimitRange, BaccaratTableState, BaccaratTableDisplayInfo} from './game-state';
import {Messenger} from './messenger';

import * as UI from './ui-messages';

import {BetLimitDialog} from './bet-limit-popup';
import {HttpClient} from './http-client';
import {App} from './app';

@autoinject()
export class LobbyPage {

    merchantCode: string = "";
    playerName: string = "";
    playerCurrency: string = "";
    playerBalance: number = 0;

    alertMessage: string = null;

    selectedTableCode: string = "";
    betLimitOptions: Array<string> = [];

    tables: Array<BaccaratTableDisplayInfo> = [];
    workingTables: Array<BaccaratTableDisplayInfo> = [];
    minVisibleTableCount: number = 0;

    countdownTimer: any = null;

    subscribers: Array<Subscription> = [];

    constructor(public dialogService: DialogService, public router: Router, 
                public i18n: I18N, public gameState: GameState, 
                public messenger: Messenger,
                public eventChannel: EventAggregator) {

        this.subscribers = [];

        
    }

    attached() {
        this.subscribers = [];
        this.subscribers.push(this.eventChannel.subscribe(UI.LoginInfo, data => {
            this.playerCurrency = this.gameState.playerCurrency;
            this.playerBalance = this.gameState.currentPlayerBalance;
            this.merchantCode = this.gameState.merchantName;
            this.playerName = this.gameState.playerId;
        }));
        this.subscribers.push(this.eventChannel.subscribe(UI.TableInfoUpdate, data => {
            this.tables = [];
            this.workingTables = [];
            let tableCodes = Array.from(this.gameState.baccaratTableStates.keys());
            for (let tableCode of tableCodes) {
                let tableState = this.gameState.baccaratTableStates.get(tableCode);
                let tableInfo = new BaccaratTableDisplayInfo();
                tableInfo.tableCode = tableState.tableCode;
                tableInfo.countdown = tableState.betTimeCountdown;
                tableInfo.isOpen = tableState.roundState > 1;
                tableInfo.serverCode = tableState.gameServer;
                tableInfo.state = BaccaratTableState.SIMPLE_GAME_STATES[tableState.roundState];
                if (tableInfo.isOpen) {
                    this.workingTables.push(tableInfo);
                    tableInfo.simpleRoadmap = JSON.parse(JSON.stringify(tableState.simpleRoadmap));
                } else {
                    tableInfo.simpleRoadmap = BaccaratTableState.createEmptySimpleRoadmap();
                }
                this.tables.push(tableInfo);
            }
        }));
        this.subscribers.push(this.eventChannel.subscribe(UI.TableStateUpdate, data => {
            // ...
        }));
        this.subscribers.push(this.eventChannel.subscribe(UI.PlayerMoney, data => this.playerBalance = data.value));
        this.subscribers.push(this.eventChannel.subscribe(UI.JoinGameError, data => this.alertMessage = data.message));
        this.subscribers.push(this.eventChannel.subscribe(UI.JoinGameSuccess, data => this.router.navigate("baccarat")));
    }

    detached() {
        for (let item of this.subscribers) item.dispose();
        this.subscribers = [];
    }

    activate(parameters, routeConfig) {

        this.gameState.currentPage = "lobby";
        
        this.changeLang(this.gameState.currentLang);

        this.merchantCode = this.gameState.merchantName;
        this.playerName = this.gameState.playerId;

        if (this.playerCurrency.length <= 0) {
            this.playerCurrency = this.gameState.playerCurrency;
            this.playerBalance = this.gameState.playerMoney;
        }

        this.playerBalance = this.gameState.currentPlayerBalance;
        if (this.playerCurrency.length <= 0) this.playerCurrency = "CNY";

        if (this.tables.length <= 0) {
            this.workingTables = [];
            this.gameState.baccaratTableStates.forEach((table) => {
                let tableInfo = new BaccaratTableDisplayInfo();
                tableInfo.tableCode = table.tableCode;
                tableInfo.countdown = table.betTimeCountdown;
                tableInfo.isOpen = table.roundState > 1;
                tableInfo.serverCode = table.gameServer;
                tableInfo.state = BaccaratTableState.SIMPLE_GAME_STATES[table.roundState];
                if (tableInfo.isOpen) {
                    this.workingTables.push(tableInfo);
                    tableInfo.simpleRoadmap = JSON.parse(JSON.stringify(table.simpleRoadmap));
                } else {
                    tableInfo.simpleRoadmap = BaccaratTableState.createEmptySimpleRoadmap();
                }
                this.tables.push(tableInfo);
            });
        }

        this.messenger.processPendingMessages("lobby");

        if (this.countdownTimer != null) {
            clearInterval(this.countdownTimer);
            this.countdownTimer = null;
        }
        this.countdownTimer = setInterval(() => {
            // ...
        }, 1000);
        
    }

    deactivate() {
        if (this.countdownTimer != null) {
            clearInterval(this.countdownTimer);
            this.countdownTimer = null;
        }
    }

    get canShowTables() {
        return this.workingTables.length > 0 && (this.minVisibleTableCount <= 0 
                || this.workingTables.length >= this.minVisibleTableCount 
                || this.tables.length < this.minVisibleTableCount);
    }

    get isEmptyAlertMessage() {
        return this.alertMessage == null || this.alertMessage.length <= 0;
    }

    dismissAlertMessage() {
        this.alertMessage = null;
    }

    updateDealerInfo(tableState: TableState) {
        if (!tableState.isOpen) {
            tableState.dealerName = "";
            tableState.dealerPhotoUrl = "";
            return;
        }
        HttpClient.getJSON(App.config.dealerInfoUrl + "?id=" + tableState.dealerId + "&photo=1&callback=?", null,
        (info) => {
            console.log(info);
            tableState.dealerName = info.Name;
            tableState.dealerPhotoUrl = App.config.dealerInfoHost + info.Photo; // need dealerInfoHost only for test
        }, () => {
            console.log("Failed to get dealer info");
        });
    }

    showBetLimits(tableCode: string) {
        this.selectedTableCode = tableCode;
        this.betLimitOptions = [];
        let talbeBetLimitList: Array<BetLimitRange> = this.gameState.talbeBetLimits.get(this.selectedTableCode);
        if (talbeBetLimitList != undefined && talbeBetLimitList != null) {
            for (let talbeBetLimitItem of talbeBetLimitList) {
                this.betLimitOptions.push(talbeBetLimitItem.min + " - " + talbeBetLimitItem.max);
            }
        }
        
        if (this.betLimitOptions.length <= 0) return;

        this.dialogService.open({viewModel: BetLimitDialog, model: this.betLimitOptions })
        .whenClosed((response) => {
            console.log(response);
            if (!response.wasCancelled && response.output.length > 0) {
                this.enterTable(response.output);
            } else {
                console.log('Give up joining table');
            }
        });
    }

    enterTable(betLimitIndex: number) {
        console.log("going to enter " + this.selectedTableCode + " with " + this.betLimitOptions[betLimitIndex]);
        this.gameState.selectedTableCode = this.selectedTableCode;
        this.gameState.selectedBetLimitIndex = betLimitIndex;
        for (let tableState of this.tables) {
            if (tableState.tableCode == this.selectedTableCode) {
                //this.messenger.joinGameTable(tableState);
                break;
            }
        }
    }

    enterBaccaratTable(tableCode: string) {
        this.selectedTableCode = tableCode;
        console.log("going to enter " + this.selectedTableCode);
    }

    changeLang(lang: string) {
        this.i18n.setLocale(lang)
        .then(() => {
            this.gameState.currentLang = this.i18n.getLocale();
            console.log(this.gameState.currentLang);
        });
    }
}
