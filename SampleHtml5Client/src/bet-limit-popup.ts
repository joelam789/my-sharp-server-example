import {autoinject} from 'aurelia-framework';
import {DialogController} from 'aurelia-dialog';

@autoinject()
export class BetLimitDialog {

    betLimitOptions: Array<string> = [];

    constructor(public controller: DialogController){
        //this.controller.settings.centerHorizontalOnly = true;
    }

    activate(betLimitOptionList) {
        this.betLimitOptions = JSON.parse(JSON.stringify(betLimitOptionList));
    }
}
