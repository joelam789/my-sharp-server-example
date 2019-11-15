
import { HttpClient } from "../http-client";
import { BasePanel  } from "../ui-common/base-panel";

export class MainPanel extends BasePanel  {

    mainFrame: any = null;

    init(callback?) {
        super.init(() => {
            if (this.gui) {
                this.gui({
                    cache: false
                });
            }
            this.mainFrame = document.getElementById("main-frame");
            if (callback) callback();
        });
    }

    go(url: string) {
        if (this.mainFrame) this.mainFrame.src = url;
    }
}
