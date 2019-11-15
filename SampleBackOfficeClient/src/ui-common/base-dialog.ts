import { Widget } from "./widget";

export class BaseDialog implements Widget {

    uid: string = "";
    gui: any = null;

    constructor(uid) {
        this.uid = uid;
    }

    init(callback?) {
        this.gui = ($('#' + this.uid) as any).dialog.bind($('#' + this.uid));
        if (callback) callback();
    }

}

export class BaseCommonDialog extends BaseDialog  {

    init(callback?) {
        super.init(() => {
            if (this.gui) {
                this.gui({
                    cache: false,
                });
            }
            if (callback) callback();
        });
    }

    open() {
        if (this.gui) this.gui('open');
    }

    close() {
        if (this.gui) this.gui('close');
    }

}

