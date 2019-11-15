import { Widget } from "./widget";
import { PageEntry, BasePageEntry } from "../page-entry";

export class Container {

    private _loadings = [];

    entry: PageEntry = null;

    constructor(entry?: PageEntry) {

        if (entry) this.entry = entry;
        else this.entry = new BasePageEntry();

        $.extend(($.fn as any).form.methods, {
            getData: function(jq, params) {
                let formArray = jq.serializeArray();
                let oRet = {};
                for (let i in formArray) {
                    if (typeof(oRet[formArray[i].name]) == 'undefined') {
                        if (params) {
                            oRet[formArray[i].name] = (formArray[i].value == "true" || formArray[i].value == "false") 
                                                        ? formArray[i].value == "true" : formArray[i].value;
                        } else oRet[formArray[i].name] = formArray[i].value;
                    } else {
                        if (params) {
                            oRet[formArray[i].name] = (formArray[i].value == "true" || formArray[i].value == "false") 
                                                        ? formArray[i].value == "true" : formArray[i].value;
                        } else oRet[formArray[i].name] += "," + formArray[i].value;
                    }
                }
                return oRet;
            }
        });

    }

    getUrlParam(name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        let regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)");
        let results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }

    load(widgets: Array<Widget>, callback: ()=>void) {
        this._loadings = [];
        this._loadings.push(...widgets);
        this.loadWidgetsOneByOne(callback);
    }

    private loadWidgetsOneByOne(callback: ()=>void) {
        if (this._loadings.length <= 0) {
            if (callback) callback();
        } else {
            let widget = this._loadings.shift();
            if (widget) {
                widget.init(() => this.loadWidgetsOneByOne(callback));
            } else {
                if (callback) callback();
            }
        }
    }
}
