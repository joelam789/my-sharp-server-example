import { Widget } from "./widget";

export class BaseGrid implements Widget {

    uid: string = "";
    gui: any = null;

    constructor(uid) {
        this.uid = uid;
    }
    
    init(callback?) {
        this.gui = ($('#' + this.uid) as any).datagrid.bind($('#' + this.uid));
        if (callback) callback();
    }
}
