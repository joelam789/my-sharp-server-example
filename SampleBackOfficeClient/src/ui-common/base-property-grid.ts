import { Widget } from "./widget";

export class BasePropertyGrid implements Widget {

    uid: string = "";
    gui: any = null;

    constructor(uid) {
        this.uid = uid;
    }
    
    init(callback?) {
        this.gui = ($('#' + this.uid) as any).propertygrid.bind($('#' + this.uid));
        if (callback) callback();
    }
}
