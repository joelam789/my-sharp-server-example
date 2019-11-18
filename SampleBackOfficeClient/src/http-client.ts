// for now, we are using jquery (coz jquery has been included for bootstrap already) to send http requests...
// we might change to use "axios" or "whatwg-fetch(github/fetch)" or aurelia-fetch-client" if do not need jquery anymore

export class HttpClient {
    
    static getJSON(url: string, data?: any, callback?: (reply: any)=>void, onerror?: (errmsg: string)=>void) {

        let requrl = HttpClient.getUrl(url);
        let sessionId = (window as any).appConfig.sessionId;

        if (!sessionId || sessionId.length <= 0) {
            if (window.parent) sessionId = (window.parent as any).appConfig.sessionId;
        }
        if (!sessionId || sessionId.length <= 0) sessionId = "";
        if (requrl.indexOf('?') > 0) requrl += "&sessionId=" + sessionId;
        else requrl += "?sessionId=" + sessionId;

        $.getJSON(requrl, data, (ret) => {
            //console.log(ret);
            if (callback != null) callback(ret);
        })
        .fail((jqxhr, textStatus, error) => {
            console.log(jqxhr);
            console.log(textStatus);
            console.log(error);
            console.log("Failed to get JSON from URL: " + url);
            if (onerror != null) onerror(textStatus);
        });
    }

    static postJSON(url: string, data?: any, callback?: (reply: any)=>void, onerror?: (errmsg: string)=>void) {
        let requrl = HttpClient.getUrl(url);
        let sessionId = (window as any).appConfig.sessionId;
        if (!sessionId || sessionId.length <= 0) {
            if (window.parent) sessionId = (window.parent as any).appConfig.sessionId;
        }
        if (!sessionId || sessionId.length <= 0) sessionId = "";
        
        //data["sessionId"] = sessionId;
        if (data) data["sessionId"] = sessionId;
        else {
            if (requrl.indexOf('?') > 0) requrl += "&sessionId=" + sessionId;
            else requrl += "?sessionId=" + sessionId;
        }
        
        let t = (requrl == (window as any).appConfig.loginReqUrl) ? 3000 : 0;
        $.ajax({
            url: requrl,
            type: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'text/plain;charset=utf-8'
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
                ($ as any).messager.alert('Error', "Network error, please re-try later", 'error');
                console.log(xhr);
                console.log(status);
                let errmsg = "Failed to post JSON to URL: " + url;
                console.log(errmsg);
                if (onerror) onerror("post json error");
                callback(xhr.status);
            },
            timeout: t
        });
    }
    
    static postForm(url: string, data?: any){
        let requrl = HttpClient.getUrl(url);
        let win = window.open();
        let c = '<html><head><meta charset="utf-8" /></head><body>'
        + '<form action="'+requrl+'" method="get" name="myForm" target="_blank" enctype="text/plain">';
        for ( var k in data){
            var v = data[k];
            c += '<input type="hidden" value="'+v+'" name="'+k+'" />';
        }
        c += '</form>'
		+ '<script>function closeWin(){window.close();}document.myForm.submit();window.setTimeout(\'closeWin()\',3000);</s'+'cript>'
        + '</body></html>';
        
        win.document.write(c);
        try			{	console.log(jQuery.parseXML(c));	}
        catch(e)	{	console.log(c);						}
    }

    static uploadFile(url:string, formData: any, that: any, textId: string, statusId: string, title: string){
        
        let sessionId = (window as any).appConfig.sessionId;
        if (!sessionId || sessionId.length <= 0) {
            if (window.parent) sessionId = (window.parent as any).appConfig.sessionId;
        }
        if (!sessionId || sessionId.length <= 0) sessionId = "";
        url += '&sessionId=' + sessionId;

        //data["sessionId"] = sessionId;
        let requrl = HttpClient.getUrl(url);
        $.ajax({
            type: "POST",
            url: requrl,
            xhr: function () {
                var myXhr = $.ajaxSettings.xhr();
                if (myXhr.upload) {
                    myXhr.upload.addEventListener('progress', that.progressHandling, false);
                }
                return myXhr;
            },
            success: function (data) {
                data = (JSON as any).parse(data);
                var json = data;
                if (json && json.error === 0) {
                    if (json && json.message)                       
                    {
                        ($ as any).messager.alert('Success', json.message, 'info');      
                        $(textId).val('');
                        $(statusId).text("Done!!").css('left','35px');
                    }
                    else{ ($ as any).messager.alert(title, "error [-1]", 'info'); }
                } else {

                    if (json){
                        ($ as any).messager.alert(title, json.message, 'info');
                    }

                }

            },
            error: function (xhr, status) {
                //
            },
            async: true,
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            timeout: 7200000
        });

    }

    static getUrl(url: string) {
        if (url.indexOf("http://") < 0 && url.indexOf("https://") < 0) {
            return (window as any).appConfig.domainApiUrl 
                ? window.location.protocol + "//" + (window as any).appConfig.domainApiUrl + url
                : url
        } else return url;
    }


}
