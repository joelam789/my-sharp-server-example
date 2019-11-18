
import { HttpClient } from "./http-client";

export interface PageEntry {
    validate(url: string, account: any, callback: (result?: any)=>void );
}

export class BasePageEntry implements PageEntry {

    validate(url: string, account: any, callback: (result?: any)=>void ) {
        if (url && url.length > 0) {
            HttpClient.postJSON(url, account, (json) => {
                if (callback) callback(json);
            }, (err) => {
                if (callback) callback(null);
            });
        } else {
            if (callback) callback( {error: 0} );
        }
    }
}
