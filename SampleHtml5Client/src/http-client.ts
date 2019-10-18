// for now, we are using jquery (coz jquery has been included for bootstrap already) to send http requests...
// we might change to use "axios" or "whatwg-fetch(github/fetch)" or aurelia-fetch-client" if do not need jquery anymore

export class HttpClient {
    
    /*
    static getJSON(url: string, callback: (json: any)=>void = null, onerror: ()=>void = null) {
        $.getJSON(url, (data) => {
            if (callback != null) callback(data);
        })
        .fail(() => {
            if (onerror != null) onerror();
        });
    }
    */

   static getJSON(url: string, data?: any, callback?: (json: any)=>void, onerror?: (errmsg?: string)=>void) {
        $.getJSON(url, data, (ret) => {
            if (callback != null) callback(ret);
        })
        .fail((jqxhr, textStatus, error) => {
            console.log(jqxhr);
            console.log(textStatus);
            console.log(error);
            if (onerror != null) onerror(textStatus);
        });
    }

    static sendRequest(url: string, callback?: (json: any)=>void, onerror?: (errmsg?: string)=>void) {
        $.getJSON(url, (ret) => {
            if (callback != null) callback(ret);
        })
        .fail((jqxhr, textStatus, error) => {
            console.log(jqxhr);
            console.log(textStatus);
            console.log(error);
            if (onerror != null) onerror(textStatus);
        });
    }

    static postJSON(url: string, data?: any, callback?: (reply: any)=>void, onerror?: (errmsg: string)=>void) {
        let requrl = url;
        //data["sessionId"] = sessionId;
        $.ajax({
            url: requrl,
            type: "POST",
            headers: {
                'Accept': 'text/plain',
                'Content-Type': 'text/plain'
            },
            crossDomain: true,
            data: JSON.stringify(data),
            dataType: "text",
            success: (response) => {
                let json = null;
                try {
                    json = JSON.parse(response);
                } catch(err) {
                    console.log("parse json error: ");
                    console.log(err);
                }
                if (callback) {
                    if (json) callback(json);
                    else callback(response);
                }
            },
            error: (xhr, status) => {
                console.log(xhr);
                console.log(status);
                let errmsg = "Failed to post JSON to URL: " + url;
                console.log(errmsg);
                if (onerror) onerror("post json error");
                callback(xhr.status);
            }
        });
    }

    /*
    static get(url: string, config?: any, callback?: (data: any)=>void, onerror?: (err: any)=>void) {
        axios.get(url, config)
        .then(function (response) {
            if (callback) callback(response);
        })
        .catch(function (error) {
            console.log(error);
            if (onerror) onerror(error);
        });
    }
    
    static fetch(url: string, config?: any, callback?: (data: any)=>void, onerror?: (err: any)=>void) {
        fetch(url, config)
        .then(function(response) {
            if (callback) callback(response);
        }).catch(function(ex) {
            console.log(ex);
            if (onerror) onerror(ex);
        });
    }
    */
    
}
