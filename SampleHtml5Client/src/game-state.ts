
export enum BaccaratPool {
    Tie = 0,
    PlayerPair,
    BankerPair,
    Player,
    Banker
}

export class GameTable {
    tableCode: string = "";
    tableName: string = "";
    phoneNumber: string = "";
}

export class BaccaratTableState {

    static readonly SIMPLE_ROADMAP_ROW_COUNT = 5;
    static readonly SIMPLE_ROADMAP_COL_COUNT = 10;

    static readonly SIMPLE_GAME_STATES = ["Unknown", "Closed", "Prepare", "NewRound", "BettingTime", 
                                        "Dealing", "Dealing", "Dealing", "Counting", "Settling"];

    gameServer: string = "";
    tableCode: string = "";
    shoeCode: string = "";
    roundNumber: number = 0;
    roundState: number = 0;
    roundStateText: string = "";
    betTimeCountdown: number = 0;
    playerCards: string = "";
    bankerCards: string = "";
    gameResult: string = "";
    gameResultHistory: string = "";

    simpleHistory: string = "";
    simpleRoadmap: BaccaratRoadmapCell[][] = null;

    constructor() {
        
        this.simpleRoadmap = new Array<Array<BaccaratRoadmapCell>>();
        for (let col = 0; col < BaccaratTableState.SIMPLE_ROADMAP_COL_COUNT; col++) {
            let roadmap = new Array<BaccaratRoadmapCell>();
            for (let row = 0; row < BaccaratTableState.SIMPLE_ROADMAP_ROW_COUNT; row++) {
                roadmap.push(new BaccaratRoadmapCell());;
            }
            this.simpleRoadmap.push(roadmap);
        }
    }

    resetSimpleRoadmap() {
        let maxX = BaccaratTableState.SIMPLE_ROADMAP_COL_COUNT;
        let maxY = BaccaratTableState.SIMPLE_ROADMAP_ROW_COUNT;
        for (let col = 0; col < maxX; col++) {
            for (let row = 0; row < maxY; row++) {
                this.simpleRoadmap[col][row].red = false;
                this.simpleRoadmap[col][row].blue = false;
                this.simpleRoadmap[col][row].green = 0;
            }
        }
    }

    updateSimpleRoadmap(historyString: string): boolean {

        if (this.simpleHistory == historyString) return false;
        if (this.simpleRoadmap == null || this.simpleRoadmap.length <= 0) return false;
        if (this.simpleRoadmap[0] == null || this.simpleRoadmap[0].length <= 0) return false;

        let roadmap = new Array<Array<BaccaratRoadmapCell>>();
        let historyList = historyString.split(',');

        for (let item of historyList) {

            if (item.length <= 0) continue;
            let history = parseInt(item);
            if (isNaN(history)) continue;

            let lastcol = roadmap.length > 0 ? roadmap[roadmap.length - 1] : null;
            if (lastcol == null) {
                lastcol = new Array<BaccaratRoadmapCell>();
                if (lastcol != null) roadmap.push(lastcol);
            }
            if (lastcol == null) break;

            let lastcell = lastcol.length <= 0 ? null : lastcol[lastcol.length - 1];

            let simpleResult = BaccaratTableState.getWinlossFlagFromResult(history.toString());
            if (simpleResult == 3) {
                //if (lastcell == null || (!lastcell.blue && !lastcell.red)) continue;
                if (lastcell == null) {
                    lastcell = new BaccaratRoadmapCell();
                    lastcol.push(lastcell);
                }
                lastcell.green++;
            } else if (simpleResult == 2) {
                if (lastcell != null && !lastcell.red && !lastcell.blue) {
                    lastcell.blue = true;
                    continue;
                }
                let cell = new BaccaratRoadmapCell();
                cell.blue = true;
                if (lastcell != null && lastcell.red) {
                    lastcol = new Array<BaccaratRoadmapCell>();
                    roadmap.push(lastcol);
                }
                lastcol.push(cell);
            } else if (simpleResult == 1) {
                if (lastcell != null && !lastcell.red && !lastcell.blue) {
                    lastcell.red = true;
                    continue;
                }
                let cell = new BaccaratRoadmapCell();
                cell.red = true;
                if (lastcell != null && lastcell.blue) {
                    lastcol = new Array<BaccaratRoadmapCell>();
                    roadmap.push(lastcol);
                }
                lastcol.push(cell);
            }
        }

        if (roadmap.length <= 0 || roadmap[0].length <= 0) {
            this.resetSimpleRoadmap();
            return true;
        }

        let start = 0;
        let unfit = false;
        let maxX = BaccaratTableState.SIMPLE_ROADMAP_COL_COUNT;
        let maxY = BaccaratTableState.SIMPLE_ROADMAP_ROW_COUNT;

        while (start < roadmap.length) {

            this.resetSimpleRoadmap();

            for (let col = start; col < roadmap.length; col++) {
                let road = roadmap[col];
                let roadIndex = col-start;
                if (roadIndex >= maxX) {
                    unfit = true;
                    break;
                }
                let lastX = roadIndex;
                let lastY = -1;
                let direction = 'y';
                for (let row = 0; row < road.length; row++) {
                    if (row == 0) { // if it's the first cell of the road
                        if (this.simpleRoadmap[roadIndex][row].red || this.simpleRoadmap[roadIndex][row].blue) {
                            unfit = true;
                            break;
                        } else {
                            this.simpleRoadmap[roadIndex][row].red = roadmap[col][row].red;
                            this.simpleRoadmap[roadIndex][row].blue = roadmap[col][row].blue;
                            this.simpleRoadmap[roadIndex][row].green = roadmap[col][row].green;
                            lastX = roadIndex;
                            lastY = row;
                            continue;
                        }
                    } else {
                        let nextX = direction == 'y' ? lastX : lastX + 1;
                        let nextY = direction == 'y' ? lastY + 1 : lastY;
                        let isOutside = nextX >= maxX || nextY >= maxY;
                        let isOccupied = isOutside || (this.simpleRoadmap[nextX][nextY].red || this.simpleRoadmap[nextX][nextY].blue);
                        let changedDir = false;
                        if ((isOutside || isOccupied) && !changedDir) {
                            changedDir = true;
                            direction = direction == 'y' ? 'x' : 'y';
                            nextX = direction == 'y' ? lastX : lastX + 1;
                            nextY = direction == 'y' ? lastY + 1 : lastY;
                            isOutside = nextX >= maxX || nextY >= maxY;
                            isOccupied = isOutside || (this.simpleRoadmap[nextX][nextY].red || this.simpleRoadmap[nextX][nextY].blue);
                        }
                        if ((isOutside || isOccupied) && changedDir) {
                            unfit = true;
                            break;
                        } else {
                            this.simpleRoadmap[nextX][nextY].red = roadmap[col][row].red;
                            this.simpleRoadmap[nextX][nextY].blue = roadmap[col][row].blue;
                            this.simpleRoadmap[nextX][nextY].green = roadmap[col][row].green;
                            lastX = nextX;
                            lastY = nextY;
                            continue;
                        }
                    }
                }
                if (unfit) break;
            }

            if (!unfit) break;
            else {
                start++;
                unfit = false;
            }

        }

        if (start >= roadmap.length) unfit = true;

        if (unfit) {
            console.log(this.tableCode + " - Failed to update simple roadmap: " + start);
            return false;
        }
        
        this.simpleHistory = historyString;

        return true;
    }

    static getPlayerScoreFromResult(result: string): number {
        return parseInt(result.substr(2, 1), 10);
    }

    static getBankerScoreFromResult(result: string): number {
        return parseInt(result.substr(1, 1), 10);
    }

    static getWinlossFlagFromResult(result: string): number {
        return parseInt(result.substr(0, 1), 10);
    }

    static createEmptySimpleRoadmap() {
        let simpleEmptyRoadmap = new Array<Array<BaccaratRoadmapCell>>();
        for (let col = 0; col < BaccaratTableState.SIMPLE_ROADMAP_COL_COUNT; col++) {
            let roadmap = new Array<BaccaratRoadmapCell>();
            for (let row = 0; row < BaccaratTableState.SIMPLE_ROADMAP_ROW_COUNT; row++) {
                roadmap.push(new BaccaratRoadmapCell());;
            }
            simpleEmptyRoadmap.push(roadmap);
        }
        return simpleEmptyRoadmap;
    }
}

export class BaccaratTableDisplayInfo {
    state: string = "";
    tableCode: string = "";
    serverCode: string = "";
    countdown: number = 0;
    isOpen: boolean = false;
    simpleRoadmap: Array<Array<any>> = new Array<Array<any>>();
}

export class TableState {
    state: string = "";
    tableCode: string = "";
    tableName: string = "";
    phoneNumber: string = "";
    isOpen: boolean = false;
    serverCode: string = "";
    gameType: number = 0;
    tableType: number = 0;
    anchorId: number = 0;
    dealerId: number = 0;
    dealerName: string = "";
    dealerPhotoUrl: string = "";
    simpleRoadmap: Array<Array<any>> = new Array<Array<any>>();
    countdown: number = 0;
}

export class BetLimitRange {
    min: number = 0;
    max: number = 0;
}

export class GameCard {
    suit: number = -1; // CARD_SUIT_SPADE = 0, CARD_SUIT_HEART, CARD_SUIT_CLUB, CARD_SUIT_DIAMOND
    value: number = -1; // CARD_ACE = 0, CARD_2, CARD_3, ... , CARD_10, CARD_JACK, CARD_QUEEN, CARD_KING
    open: boolean = false;
}

export class BaccaratRoadmapCell {
    red: boolean = false;
    blue: boolean = false;
    green: number = 0;
}

export class BaccaratState {

    static readonly SIMPLE_ROADMAP_ROW_COUNT = 5;
    static readonly SIMPLE_ROADMAP_COL_COUNT = 10;

    state: string = ""; // GAME_STATE_CLOSED = 0, GAME_STATE_BETTING, GAME_STATE_DEALING, GAME_STATE_PREPARING
    stateTimeLeft: number = 0;

    tableCode: string = "";
    dealerId: string = "";
    dealerName: string = "";

    gameId : string = "";
    roundId: number = 0;
    roundCount: number = 0;
    shoeCount: number = 0;

    isHolding: boolean = false;

    playerScore: number = 0;
    bankerScore: number = 0;

    winningPools: number = 0;

    playerCards: Array<GameCard> = [new GameCard(), new GameCard(), new GameCard()];
    bankerCards: Array<GameCard> = [new GameCard(), new GameCard(), new GameCard()];

    acceptedBetAmount: Map<BaccaratPool, number> = new Map<BaccaratPool, number>();

    simpleHistory: string = "";
    simpleRoadmap: BaccaratRoadmapCell[][] = null;

    constructor() {
        this.resetBetAmounts();

        this.simpleRoadmap = new Array<Array<BaccaratRoadmapCell>>();
        for (let col = 0; col < BaccaratState.SIMPLE_ROADMAP_COL_COUNT; col++) {
            let roadmap = new Array<BaccaratRoadmapCell>();
            for (let row = 0; row < BaccaratState.SIMPLE_ROADMAP_ROW_COUNT; row++) {
                roadmap.push(new BaccaratRoadmapCell());;
            }
            this.simpleRoadmap.push(roadmap);
        }
    }

    resetBetAmounts() {
        this.acceptedBetAmount.set(BaccaratPool.Tie, 0);
        this.acceptedBetAmount.set(BaccaratPool.Player, 0);
        this.acceptedBetAmount.set(BaccaratPool.PlayerPair, 0);
        this.acceptedBetAmount.set(BaccaratPool.Banker, 0);
        this.acceptedBetAmount.set(BaccaratPool.BankerPair, 0);
    }

    static createEmptySimpleRoadmap() {
        let simpleEmptyRoadmap = new Array<Array<BaccaratRoadmapCell>>();
        for (let col = 0; col < BaccaratState.SIMPLE_ROADMAP_COL_COUNT; col++) {
            let roadmap = new Array<BaccaratRoadmapCell>();
            for (let row = 0; row < BaccaratState.SIMPLE_ROADMAP_ROW_COUNT; row++) {
                roadmap.push(new BaccaratRoadmapCell());;
            }
            simpleEmptyRoadmap.push(roadmap);
        }
        return simpleEmptyRoadmap;
    }

}

export class GameState {

    // some global ui info
    currentPage: string = "";
    currentLang: string = "en";

    // for logging in to login server
    merchantName: string = "";
    playerId: string = "";
    password: string = "";
    serverUrl: string = "";
    loginError: string = "";

    // for logging in to frontend server
    companyCode: string = "";
    playerName: string = "";
    loginSystemId: number = 0;
    loginKey: string = "";
    hostIp: string = "";
    hostPort: number = 0;

    systemType: number = 0;

    // login info
    playerCurrency: string = "";
    playerReloginToken: string = "";
    playerMoney: number = 0;

    betLimitProfiles: any = null;
    playerBetHistoryUrl: string = "";

    selectedTableCode: string = "";
    selectedBetLimitIndex: number = -1;

    currentTableCode: string = "";

    // info from login server
    loginServerAddress: string = "";
    frontEndServerAddress: string = "";
    bettingServerAddress: string = "";
    currentPlayerSessionId: string = "";
    currentPlayerBalance: number = 0;

    // info from front-end server
    frontEndServerName: string = "";
    frontEndClientId: string = "";
    baccaratTableStates: Map<string, BaccaratTableState> = new Map<string, BaccaratTableState>();

    // table info
    tables: Array<GameTable> = [];

    talbeBetLimits: Map<string, Array<BetLimitRange>> = new Map<string, Array<BetLimitRange>>();
    talbeChips: Map<string, Array<Array<number>>> = new Map<string, Array<Array<number>>>();
    
    tableStates: Map<string, TableState> = new Map<string, TableState>();

    baccaratStates: Map<string, BaccaratState> = new Map<string, BaccaratState>();

    countdownTimer: any = null;

    startAutoCountdown() {
        if (this.countdownTimer != null) {
            clearInterval(this.countdownTimer);
            this.countdownTimer = null;
        }
        this.countdownTimer = setInterval(() => {
            this.tableStates.forEach((tableState) => {
                if (tableState.isOpen && tableState.countdown > 0) tableState.countdown--;
                if (!tableState.isOpen || tableState.countdown < 0) tableState.countdown = 0;
            });
        }, 1000);
    }

    stopAutoCountdown() {
        if (this.countdownTimer != null) {
            clearInterval(this.countdownTimer);
            this.countdownTimer = null;
        }
    }

    updateTableInfo() {
        if (this.betLimitProfiles == null) return;
        for (let table of this.tables) {

            let tableState = this.tableStates.get(table.tableCode);
            if (tableState == undefined || tableState == null) {
                tableState = new TableState();
            }

            tableState.tableCode = table.tableCode;
            tableState.tableName = table.tableName;
            tableState.phoneNumber = table.phoneNumber;
            this.tableStates.set(table.tableCode, tableState);

            let tableCode = table.tableCode;
            let talbeBetLimitList = [];
            let talbeChipList = [];

            
        }
    }

    updateBaccaratSimpleRoadmap(tableCode: string, historyString: string) {
        let baccaratState = this.baccaratStates.get(tableCode);
        if (baccaratState != undefined && baccaratState != null) {
            
        }
    }

    resetBaccaratState(tableCode: string) {
        if (tableCode == undefined || tableCode == null || tableCode.length <= 0) return;
        this.baccaratStates.set(tableCode, new BaccaratState());
        this.baccaratStates.get(tableCode).tableCode = tableCode;
    }

    getCardCode(card: GameCard = null): string {
        if (card != null && card.suit >= 0 && card.value >= 0) {
            let cardNamePart1Chars = ['1', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K'];
            let cardNamePart2Chars = ['S', 'H', 'C', 'D'];
            if (card.suit < cardNamePart2Chars.length && card.value < cardNamePart1Chars.length) {
                return cardNamePart1Chars[card.value] + cardNamePart2Chars[card.suit];
            }
        }
        return "01";
    }

}
