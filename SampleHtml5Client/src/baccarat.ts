
import {autoinject} from 'aurelia-framework';
import {EventAggregator, Subscription} from 'aurelia-event-aggregator';
import {Router} from 'aurelia-router';

import {DialogService} from 'aurelia-dialog';
import {I18N} from 'aurelia-i18n';

import {GameMedia} from './game-media';

import {GameState, GameTable, BetLimitRange, BaccaratState, 
        BaccaratTableState, BaccaratPool, BaccaratTableGameInfo, GameCard} from './game-state';

import {Messenger} from './messenger';
import * as UI from './ui-messages';
import {App} from './app';

@autoinject()
export class BaccaratPage {

    merchantCode: string = "";
    playerName: string = "";
    playerCurrency: string = "";
    playerBalance: number = 0;

    tableCode: string = "";
    tableName: string = "";
    anchorId: number = 0;
    dealerId: number = 0;

    totalBet: number = 0;
    countdown: number = 0;

    isVideoReady: boolean = false;
    hasNewBets: boolean = false;

    selectedChip: string = "";
    chips: Array<number> = [];

    placedBetAmount: Map<BaccaratPool, number> = new Map<BaccaratPool, number>();
    gainAmount: Map<BaccaratPool, number> = new Map<BaccaratPool, number>();

    subscribers: Array<Subscription> = [];

    gameTableInfo: BaccaratTableGameInfo = new BaccaratTableGameInfo();
    gameTableStateText: string = "";

    playerCards: Array<GameCard> = [];
    bankerCards: Array<GameCard> = [];

    lastTableState: number = 0;

    countdownTimer: any = null;

    constructor(public dialogService: DialogService, public router: Router, 
                public i18n: I18N, public gameState: GameState, public gameMedia: GameMedia, 
                public messenger: Messenger, public eventChannel: EventAggregator) {

        this.resetBetAmounts();
        
    }

    applyTableInfo() {

        let table = this.gameState.baccaratTableStates.get(this.tableCode);
        this.lastTableState = this.gameTableInfo.basicInfo.roundState;
        this.gameTableInfo.basicInfo = JSON.parse(JSON.stringify(table.basicInfo));
        this.gameTableInfo.simpleRoadmap = JSON.parse(JSON.stringify(table.simpleRoadmap));

        this.countdown = this.gameTableInfo.basicInfo.betTimeCountdown;
        this.gameTableStateText = BaccaratTableState.SIMPLE_GAME_STATES[this.gameTableInfo.basicInfo.roundState].toLocaleLowerCase();

        let playerCardArray = this.gameTableInfo.basicInfo.playerCards.split(",");
        for (let i=0; i<playerCardArray.length; i++) {
            if (playerCardArray[i].charAt(0) == 'A') 
                playerCardArray[i] = '1' + playerCardArray[i].charAt(1);
        }
        //console.log(playerCardArray);
        this.playerCards = [];
        for (let i=1; i<=3; i++) {
            if (playerCardArray.length < i) {
                this.playerCards.push(new GameCard());
            } else {
                let card = GameState.getCardByCode(playerCardArray[i-1]);
                this.playerCards.push(card);
            }
        }
        //console.log(this.playerCards);

        let bankerCardArray = this.gameTableInfo.basicInfo.bankerCards.split(",");
        for (let i=0; i<bankerCardArray.length; i++) {
            if (bankerCardArray[i].charAt(0) == 'A') 
                bankerCardArray[i] = '1' + bankerCardArray[i].charAt(1);
        }
        //console.log(bankerCardArray);
        this.bankerCards = [];
        for (let i=1; i<=3; i++) {
            if (bankerCardArray.length < i) {
                this.bankerCards.push(new GameCard());
            } else {
                let card = GameState.getCardByCode(bankerCardArray[i-1]);
                this.bankerCards.push(card);
            }
        }
        //console.log(this.bankerCards);

    }

    attached() {

        this.subscribers = [];

        this.subscribers.push(this.eventChannel.subscribe(UI.TableInfoUpdate, data => {
            if (this.gameState.baccaratTableStates.has(this.tableCode)) {

                this.applyTableInfo();

                if (this.gameTableInfo.basicInfo.roundState == 4 && this.lastTableState < 4) {
                    this.resetBetAmounts();
                }

                if (this.gameTableInfo.basicInfo.roundState == 3 ) {
                    this.resetBetAmounts();
                    this.resetCardSprites();
                }

                if (this.gameTableInfo.basicInfo.roundState > 4 && this.lastTableState <= 4) {
                    this.resetCardSprites();
                }

                if (this.gameTableInfo.basicInfo.roundState != 4 ) this.updateGameCanvas();
                else this.resetCardSprites();

                if (this.gameTableInfo.basicInfo.roundState == 9) {

                    //console.log(this.gameTableInfo.basicInfo.bankerCards);
                    //console.log(this.gameTableInfo.basicInfo.playerCards);
                    //console.log(this.gameTableInfo.basicInfo.gameResult);

                    let winloss = this.gameTableInfo.basicInfo.gameResult.charAt(0);
                    this.gainAmount.set(BaccaratPool.Banker, winloss == '1' ? 1 : 0);
                    this.gainAmount.set(BaccaratPool.Player, winloss == '2' ? 1 : 0);
                    this.gainAmount.set(BaccaratPool.Tie, winloss == '3' ? 1 : 0);

                } else {

                    //this.gainAmount.set(BaccaratPool.Tie, 0);
                    //this.gainAmount.set(BaccaratPool.Player, 0);
                    //this.gainAmount.set(BaccaratPool.Banker, 0);

                }

            }
        }));

        this.subscribers.push(this.eventChannel.subscribe(UI.LeaveGameTable, data => {
            console.log(data.message);
            this.router.navigate("lobby");
        }));

        this.subscribers.push(this.eventChannel.subscribe(UI.PlaceBetError, data => {
            console.log(data.message);
            let baccaratState = this.gameState.baccaratStates.get(this.tableCode)
            if (baccaratState != undefined && baccaratState != null) {
                this.placedBetAmount.forEach((value, pool) => {
                    this.placedBetAmount.set(pool, baccaratState.acceptedBetAmount.get(pool));
                });
            }
            alert(data.message);
        }));

        this.subscribers.push(this.eventChannel.subscribe(UI.PlaceBetSuccess, data => {
            console.log(data);
            this.playerBalance = data.balance;
            let baccaratState = this.gameState.baccaratStates.get(this.tableCode)
            if (baccaratState != undefined && baccaratState != null) {
                this.placedBetAmount.forEach((value, pool) => {
                    baccaratState.acceptedBetAmount.set(pool, this.placedBetAmount.get(pool));
                });
            }
            console.log("place bet successfully");
        }));

        this.subscribers.push(this.eventChannel.subscribe(UI.BetResultUpdate, data => {
            console.log(data);
            let messages = data.messages;
            for (let item of messages) {
                if (item.bet > 0 && item.payout > item.bet) {
                    this.playerBalance = parseFloat(this.playerBalance.toString()) + item.payout;
                    console.log("WIN - " + item.payout);
                }
                else if (item.bet > 0 && item.payout == item.bet) {
                    this.playerBalance = parseFloat(this.playerBalance.toString()) + item.payout;
                    console.log("TIE - " + item.payout);
                }
                else if (item.bet > 0 && item.payout < item.bet) {
                    this.playerBalance = parseFloat(this.playerBalance.toString()) + item.payout;
                    console.log("LOSE - " + item.payout);
                }
                else console.log("BET ERROR!!");
            }
        }));
    }

    detached() {
        for (let item of this.subscribers) item.dispose();
        this.subscribers = [];
    }

    activate(parameters, routeConfig) {

        this.gameState.currentPage = "table";

        this.changeLang(this.gameState.currentLang);

        this.merchantCode = this.gameState.companyCode;
        this.playerName = this.gameState.playerName;
        this.playerCurrency = this.gameState.playerCurrency;
        this.playerBalance = this.gameState.playerMoney;

        this.tableCode = this.gameState.currentTableCode;
        this.tableName = this.tableCode;

        let tableState = this.gameState.tableStates.get(this.tableCode);
        if (tableState != undefined && tableState != null) {
            this.anchorId = tableState.anchorId;
            this.dealerId = tableState.dealerId;
            this.countdown = tableState.countdown;
            this.tableName = tableState.tableName;
        }

        this.merchantCode = this.gameState.merchantName;
        this.playerName = this.gameState.playerId;

        if (this.playerCurrency.length <= 0) {
            this.playerCurrency = this.gameState.playerCurrency;
            this.playerBalance = this.gameState.playerMoney;
        }

        this.playerBalance = this.gameState.currentPlayerBalance;
        if (this.playerCurrency.length <= 0) this.playerCurrency = "CNY";

        //let chipSetList = this.gameState.talbeChips.get(this.tableCode);
        //let selectedIndex = this.gameState.selectedBetLimitIndex;
        //if (chipSetList != undefined && chipSetList != null) {
        //    if (selectedIndex >= 0 && selectedIndex < chipSetList.length) {
        //        this.chips = [];
        //        let chipset = chipSetList[this.gameState.selectedBetLimitIndex];
        //        for (let chip of chipset) this.chips.push(chip);
        //    }
        //}

        this.chips = [];
        this.chips.push(...[1, 2, 5, 10, 20, 50, 100]);

        this.applyTableInfo();

        this.messenger.processPendingMessages("table");

        if (this.countdownTimer != null) {
            clearInterval(this.countdownTimer);
            this.countdownTimer = null;
        }
        this.countdownTimer = setInterval(() => {
            //let tableState = this.gameState.tableStates.get(this.tableCode);
            //if (tableState != undefined && tableState != null) this.countdown = tableState.countdown;
        }, 1000);

        this.gameMedia.gameContainer.style.top = "200px";
        this.gameMedia.gameContainer.style.left = "calc(50% - 300px)";
        //this.gameMedia.gameContainer.style.border = "1px solid black";

        //this.gameMedia.getSpriteGroup("baccarat").left = 10;
        //this.gameMedia.getSpriteGroup("baccarat").top = 100;

        this.isVideoReady = false;

        this.gameMedia.videoContainer.style.top = "75px";
        this.gameMedia.videoContainer.style.left = "calc(50% - 570px)";
        
        this.gameMedia.stream.open(App.config.defaultVideoSource); // use defaultVideoSource for test only
        this.gameMedia.stream.onPlay = () => {
            this.isVideoReady = true;
            this.gameMedia.videoContainer.style.display = 'block';
        };

        this.resetBetAmounts();
        this.resetCardSprites();
    }

    deactivate() {

        //this.messenger.leaveGameTable();

        if (this.countdownTimer != null) {
            clearInterval(this.countdownTimer);
            this.countdownTimer = null;
        }

        this.gameMedia.stream.close();
        this.gameMedia.videoContainer.style.display = 'none';

        this.isVideoReady = false;

        this.gameMedia.onEnterFrame = null;
        this.gameMedia.game.paused = false;
        this.gameMedia.gameContainer.style.display = 'none';
    }

    get tableState(): string {
        return this.gameTableStateText;
    }

    get countdownText(): string {
        if (this.countdown > 0) return "(" + this.countdown + ")";
        return "";
    }

    get stateText(): string {
        let currentTableState = this.tableState;
        return currentTableState.length > 0 ? this.i18n.tr("baccarat." + currentTableState) : "";
    }

    get canBet(): boolean {
        
        if (this.hasNewBets) return false;

        let baccaratState = this.tableState ? this.tableState.toLocaleLowerCase() : "";
        if (baccaratState != undefined && baccaratState != null) return baccaratState == "betting";

        return false;
    }

    get totalBetAmount(): number {
        let total = 0;
        let baccaratState = this.gameState.baccaratStates.get(this.tableCode)
        if (baccaratState != undefined && baccaratState != null) {
            baccaratState.acceptedBetAmount.forEach((value, pool) => {
                total += baccaratState.acceptedBetAmount.get(pool);
            });
        }
        return total;
    }

    get canSendBets(): boolean {
        return this.hasNewBets;
    }

    get playerPoolBet(): number {
        return this.placedBetAmount.get(BaccaratPool.Player);
    }
    get bankerPoolBet(): number {
        return this.placedBetAmount.get(BaccaratPool.Banker);
    }
    get tiePoolBet(): number {
        return this.placedBetAmount.get(BaccaratPool.Tie);
    }

    get playerPoolWinloss(): string {
        let value = this.gainAmount.get(BaccaratPool.Player);
        return value > 0 ? "(WIN)" : "";
    }
    get bankerPoolWinloss(): string {
        let value = this.gainAmount.get(BaccaratPool.Banker);
        return value > 0 ? "(WIN)" : "";
    }
    get tiePoolWinloss(): string {
        let value = this.gainAmount.get(BaccaratPool.Tie);
        return value > 0 ? "(WIN)" : "";
    }

    addBet(pool: number) {
        if (!this.canBet) return;
        let val = this.placedBetAmount.get(pool);
        if (val == undefined || val == null) return;
        let bet = parseInt(this.selectedChip);
        if (isNaN(bet)) return;
        this.placedBetAmount.set(pool, val + bet);
        this.hasNewBets = true;
    }

    getNewBets() {
        let newpart = {
            pools: [],
            bets: []
        };

        let baccaratState = this.gameState.baccaratStates.get(this.tableCode)
        console.log(baccaratState);
        //if (baccaratState != undefined && baccaratState != null && baccaratState.state == "betting") {
        if (baccaratState != undefined && baccaratState != null && this.gameTableInfo.basicInfo.roundState == 4 ) {
            this.placedBetAmount.forEach((value, pool) => {
                let diff = this.placedBetAmount.get(pool) - baccaratState.acceptedBetAmount.get(pool);
                if (diff > 0) {
                    newpart.pools.push(pool);
                    newpart.bets.push(diff);
                }
            });
        }
        return newpart;
    }

    cannelNewBets() {
        this.hasNewBets = false;
        let baccaratState = this.gameState.baccaratStates.get(this.tableCode)
        if (baccaratState != undefined && baccaratState != null) {
            this.placedBetAmount.forEach((value, pool) => {
                this.placedBetAmount.set(pool, baccaratState.acceptedBetAmount.get(pool));
            });
        }
    }

    applyNewBets() {
        this.hasNewBets = false;
        let newpart = this.getNewBets();
        if (newpart.bets.length > 0) {
            //this.messenger.placeBaccaratBet(newpart.pools, newpart.bets);
            console.log("request betting");
            this.messenger.placeBet(this.tableCode, newpart.pools[0], newpart.bets[0]);
        }
    }

    resetBetAmounts() {
        this.hasNewBets = false;
        this.placedBetAmount.set(BaccaratPool.Tie, 0);
        this.placedBetAmount.set(BaccaratPool.Player, 0);
        //this.placedBetAmount.set(BaccaratPool.PlayerPair, 0);
        this.placedBetAmount.set(BaccaratPool.Banker, 0);
        //this.placedBetAmount.set(BaccaratPool.BankerPair, 0);
        this.gainAmount.set(BaccaratPool.Tie, 0);
        this.gainAmount.set(BaccaratPool.Player, 0);
        //this.gainAmount.set(BaccaratPool.PlayerPair, 0);
        this.gainAmount.set(BaccaratPool.Banker, 0);
        //this.gainAmount.set(BaccaratPool.BankerPair, 0);

        let baccaratState = this.gameState.baccaratStates.get(this.tableCode)
        if (baccaratState != undefined && baccaratState != null) {
            baccaratState.resetBetAmounts();
        }
    }

    showCardAnimation() {

        if (this.playerCards.length < 3 || this.bankerCards.length < 3) return;

        //let baccaratState = this.gameState.baccaratStates.get(this.tableCode);
        //if (baccaratState == undefined || baccaratState == null) return;
        //if (baccaratState.state == "betting") return;

        if (this.gameTableInfo == null || this.gameTableInfo.basicInfo == null 
            || this.gameTableInfo.basicInfo.roundState <= 4 ) return;

        let group = this.gameMedia.getSpriteGroup("baccarat");
        if (group == undefined || group == null) return;

        let startedNewAnimation = false;
        let foundUnfinishedAnimation = false;
        for (let i=0; i < 3; i++) {
            
            let backImage = GameState.getCardCode();
            let cardImage = "";

            let playerCard = group.getByName("P"+i);
            if (this.playerCards[i].value >= 0) {
                if (playerCard.target == null && !startedNewAnimation && !foundUnfinishedAnimation) {
                    playerCard.target = {  
                                            x: i == 2 ? 80 : 260 - 70 * i, 
                                            y: i == 2 ? 85 : 10, 
                                            image: GameState.getCardCode(this.playerCards[i]),
                                            done: false
                                        };
                    playerCard.loadTexture(backImage);
                    playerCard.x = 750;
                    playerCard.y = i == 2 ? 85 : 10;
                    this.gameMedia.game.physics.arcade.moveToXY(playerCard, 
                                                                playerCard.target.x, playerCard.target.y, 
                                                                300, 500);
                    startedNewAnimation = true;
                } else if (playerCard.target != null && playerCard.target.done == false) {
                    foundUnfinishedAnimation = true;
                }
            } else {
                playerCard.x = -300;
                playerCard.y = -300;
                playerCard.body.velocity.setTo(0, 0);
                playerCard.target = null;
            }

            let bankerCard = group.getByName("B"+i);
            if (this.bankerCards[i].value >= 0) {
                if (bankerCard.target == null && !startedNewAnimation && !foundUnfinishedAnimation) {
                    bankerCard.target = {  
                                            x: i == 2 ? 590 : 350 + 70 * i,
                                            y: i == 2 ? 20 : 10, 
                                            image: GameState.getCardCode(this.bankerCards[i]),
                                            done: false
                                        };
                    bankerCard.loadTexture(backImage);
                    bankerCard.x = 750;
                    bankerCard.y = i == 2 ? 20 : 10;
                    this.gameMedia.game.physics.arcade.moveToXY(bankerCard, 
                                                                bankerCard.target.x, bankerCard.target.y, 
                                                                300, 500);
                    startedNewAnimation = true;
                } else if (bankerCard.target != null && bankerCard.target.done == false) {
                    foundUnfinishedAnimation = true;
                }
            } else {
                bankerCard.x = -300;
                bankerCard.y = -300;
                bankerCard.body.velocity.setTo(0, 0);
                bankerCard.target = null;
            }
        }
    }

    updateCardSprites() {
        let group = this.gameMedia.getSpriteGroup("baccarat");
        if (group == undefined || group == null) return;
        let foundCompletedAnimation = false;
        for (let i=0; i < 3; i++) {
            if (foundCompletedAnimation) break;
            for (let j=0; j < 2; j++) {
                if (foundCompletedAnimation) break;
                let cardSprite = group.getByName((j % 2 == 0 ? "P" : "B") + i);
                if (cardSprite.target == undefined || cardSprite.target == null) continue;
                if (cardSprite.target.done || (cardSprite.x < 0 && cardSprite.y < 0)) continue;
                if (Math.round(cardSprite.x) <= cardSprite.target.x 
                    && Math.round(cardSprite.y) == cardSprite.target.y) {
                    cardSprite.target.done = true;
                    cardSprite.body.velocity.setTo(0, 0);
                    if (cardSprite.key != cardSprite.target.image) cardSprite.loadTexture(cardSprite.target.image);
                    foundCompletedAnimation = true;
                }
            }
        }
        if (foundCompletedAnimation) this.showCardAnimation();
            
    }

    resetCardSprites() {
        let group = this.gameMedia.getSpriteGroup("baccarat");
        if (group == undefined || group == null) return;
        for (let i=0; i < 3; i++) {
            for (let j=0; j < 2; j++) {
                let cardSprite = group.getByName((j % 2 == 0 ? "P" : "B") + i);
                cardSprite.x = -300;
                cardSprite.y = -300;
                cardSprite.body.velocity.setTo(0, 0);
                cardSprite.target = null;
            }
        }
    }

    updateGameCanvas() {

        //let baccaratState = this.gameState.baccaratStates.get(this.tableCode);
        //if (baccaratState == undefined || baccaratState == null) return;

        if (this.gameTableInfo == null || this.gameTableInfo.basicInfo == null) return;

        if (this.gameTableInfo.basicInfo.roundState <= 4 && this.gameTableInfo.basicInfo.roundState != 2) {
            
            this.gameMedia.gameContainer.style.display = 'none';
            this.gameMedia.game.paused = true;
            this.gameMedia.onEnterFrame = null;

        } else if (this.countdownTimer != null) {

            this.gameMedia.gameContainer.style.display = 'block';
            this.gameMedia.game.paused = false;

            this.showCardAnimation();

            if (this.gameMedia.onEnterFrame == null)
                this.gameMedia.onEnterFrame = () => this.updateCardSprites();
        }
    }

    changeLang(lang: string) {
        this.i18n.setLocale(lang)
        .then(() => {
            this.gameState.currentLang = this.i18n.getLocale();
            console.log(this.gameState.currentLang);
        });
    }
    
}
