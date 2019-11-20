import { Widget } from "./widget";

export class BaseDatebox implements Widget {

    uid: string = "";
    gui: any = null;

    constructor(uid) {
        this.uid = uid;
    }
    
    init(callback?) {
        this.gui = ($('#' + this.uid) as any).datebox.bind($('#' + this.uid));
        if (callback) callback();
    }
}

export class BaseDateTimebox implements Widget {

    uid: string = "";
    gui: any = null;

    constructor(uid) {
        this.uid = uid;
    }
    
    init(callback?) {
        this.gui = ($('#' + this.uid) as any).datetimebox.bind($('#' + this.uid));
        if (callback) callback();
    }
}
