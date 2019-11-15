import { Widget } from "./widget";

export class BasePanel implements Widget {

    uid: string = "";
    gui: any = null;

    constructor(uid) {
        this.uid = uid;
    }
    
    init(callback?) {
        this.gui = ($('#' + this.uid) as any).panel.bind($('#' + this.uid));
        if (callback) callback();
    }
}
