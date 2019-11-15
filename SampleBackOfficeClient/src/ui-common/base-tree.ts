import { Widget } from "./widget";

export class BaseTree implements Widget {

    gui: any = null;
    uid: string = "";

    constructor(uid) {
        this.uid = uid;
    }

    init(callback?) {
        this.gui = ($('#' + this.uid) as any).tree.bind($('#' + this.uid));
        if (callback) callback();
    }
}
