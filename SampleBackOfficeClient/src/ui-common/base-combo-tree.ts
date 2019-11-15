import { Widget } from "./widget";

export class BaseComboTree implements Widget {

    gui: any = null;
    uid: string = "";

    constructor(uid) {
        this.uid = uid;
    }

    init(callback?) {
        this.gui = ($('#' + this.uid) as any).combotree.bind($('#' + this.uid));
        if (callback) callback();
    }
}
