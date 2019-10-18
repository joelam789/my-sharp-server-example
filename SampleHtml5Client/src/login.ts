
import {autoinject} from 'aurelia-framework';
import {EventAggregator, Subscription} from 'aurelia-event-aggregator';
import {Router} from 'aurelia-router';

import {App} from './app';
import {GameMedia} from './game-media';
import {GameState} from './game-state';
import {Messenger} from './messenger';
import * as UI from './ui-messages';

@autoinject()
export class LoginPage {

    merchantName: string = "m1";
    playerId: string = "test";
    playerPassword: string = "";
    serverAddress: string = "127.0.0.1:9990";
    alertMessage: string = null;

    subscribers: Array<Subscription> = [];

    constructor(public router: Router, public gameState: GameState, public gameMedia: GameMedia, 
                public messenger: Messenger, public eventChannel: EventAggregator) {
                    
        this.merchantName = App.config.defaultMerchant;
        this.playerId = App.config.defaultPlayer;
        this.serverAddress = App.config.loginServerAddress;

    }

    attached() {
        this.subscribers = [];
        this.subscribers.push(this.eventChannel.subscribe(UI.LoginError, data => this.alertMessage = data.message));
        this.subscribers.push(this.eventChannel.subscribe(UI.LoginSuccess, data => this.router.navigate("lobby")));
    }

    detached() {
        for (let item of this.subscribers) item.dispose();
        this.subscribers = [];
    }

    activate(parameters, routeConfig) {
        console.log("done");
        this.gameState.currentPage = "login";
        this.gameMedia.loadingContainer.style.display = 'none';
        this.gameMedia.appContainer.style.display = 'block';
    }

    get canLogin() {
        return this.merchantName.length > 0 
                && this.playerId.length > 0
                && this.playerPassword.length > 0
                && this.serverAddress.length > 0
                && !this.messenger.isRequesting
                && !this.router.isNavigating;
    }

    get isEmptyAlertMessage() {
        return this.alertMessage == null || this.alertMessage.length <= 0;
    }

    dismissAlertMessage() {
        this.alertMessage = null;
    }

    connectAndLogin() {
        
        console.log("start to connect server and then try to login");
        this.gameState.merchantName = this.merchantName;
        this.gameState.playerId = this.playerId;
        this.gameState.password = this.playerPassword;
        this.gameState.loginServerAddress = this.serverAddress;
        this.messenger.login();
        
    }

    

}
