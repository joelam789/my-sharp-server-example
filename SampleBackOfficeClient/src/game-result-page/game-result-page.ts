import { BaseDateTimebox } from './../ui-common/base-datebox';

import { Container } from "../ui-common/container";
import { BaseCommonForm } from "../ui-common/base-form";

import { HttpClient } from "../http-client";

class Gui extends Container {

    //searchForm: BaseCommonForm = new BaseCommonForm("search-form");

    fromDt: BaseDateTimebox = new BaseDateTimebox("fromdt");
    toDt: BaseDateTimebox = new BaseDateTimebox("todt");

    init(callback) {
        this.load([this.fromDt, this.toDt], callback);
    }

    doSearch() {
        let fromDateTime = this.fromDt.gui('getValue');
        let toDateTime = this.toDt.gui('getValue');
        
        let data = {
            sessionId: (window as any).appConfig.sessionId,
            merchantCode: (window as any).appConfig.merchantCode,
            userId: (window as any).appConfig.userId,
            fromDateTime: fromDateTime,
            toDateTime: toDateTime
        }

        console.log(data);
    }

    
}

export const gui = new Gui();
