import { Widget } from "./widget";

export class BaseForm implements Widget {

    uid: string = "";
    gui: any = null;

    constructor(uid) {
        this.uid = uid;
    }
    
    init(callback?) {
        this.gui = ($('#' + this.uid) as any).form.bind($('#' + this.uid));
        if (callback) callback();
    }
}

export class BaseCommonForm extends BaseForm  {

    init(callback?) {
        super.init(() => {
            if (this.gui) {
                this.gui({
                    onSubmit: () => {
                        return this.onSubmit();
                    }
                });
            }
            if (callback) callback();
        });
    }

    onSubmit() {
        // do some check
        // may return false to prevent submit if necessary;
        return true;
    }

    setData(data) {
        if (this.gui) this.gui('load', data);
    }

    getData() {
        return this.gui ? this.gui('getData', true) : null;
    }
}

