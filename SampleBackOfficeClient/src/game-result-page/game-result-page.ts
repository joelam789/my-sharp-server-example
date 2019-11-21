import { BaseGrid } from './../ui-common/base-grid';
import { BaseDateTimebox } from './../ui-common/base-datebox';

import { Container } from "../ui-common/container";
import { BaseCommonForm } from "../ui-common/base-form";

import { HttpClient } from "../http-client";

class Gui extends Container {

    //searchForm: BaseCommonForm = new BaseCommonForm("search-form");

    fromDt: BaseDateTimebox = new BaseDateTimebox("fromdt");
    toDt: BaseDateTimebox = new BaseDateTimebox("todt");
    gameResultGrid: BaseGrid = new BaseGrid("game-result-grid");

    init(callback) {
        this.load([this.fromDt, this.toDt, this.gameResultGrid], () => {

            let reqUrl = window.location.protocol + "//" 
                        + (window as any).appConfig.domainApiUrl 
                        + (window as any).appConfig.gameResultReqUrl;

            this.gameResultGrid.gui({
                url: reqUrl,
                contentType: "text/plain;charset=utf-8",
                loader: function(param, success, error) {
                    HttpClient.postJSON(reqUrl , { 
                        sessionId: (window as any).appConfig.sessionId,
                        queryParam: param } , 
                        (json) => success(json), () => error()
                    );
                }
            })
            if (callback) callback();
        });
    }

    doSearch() {
        let fromDateTime = this.fromDt.gui('getValue');
        let toDateTime = this.toDt.gui('getValue');
        
        let reqParams = {
            sessionId: (window as any).appConfig.sessionId,
            merchantCode: (window as any).appConfig.merchantCode,
            userId: (window as any).appConfig.userId,
            fromDateTime: fromDateTime,
            toDateTime: toDateTime
        }

        console.log(reqParams);

        this.gameResultGrid.gui("load", reqParams);
    }

    
}

export const gui = new Gui();
