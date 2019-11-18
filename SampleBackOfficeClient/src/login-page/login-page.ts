
import { BaseCommonForm } from "../ui-common/base-form";
import { Container } from "../ui-common/container";
import { HttpClient } from "../http-client";

class Gui extends Container {

    agentForm: BaseCommonForm = new BaseCommonForm("login-form");

    selectedAgentId: number = -1;
    selectedAgentLevel: number = -1;
    connectionString:string = "";

    init(callback) {
        this.load([this.agentForm], () => {
            this.agentForm.setData({
                user_id: "",
                user_pwd: ""
            });
            if (callback) callback();
        });
    }

    login() {
        let data = this.agentForm.getData();

        if (!data || data.user_id.length <= 0 || data.user_pwd.length <= 0) {
            ($ as any).messager.alert('Login','Please input user ID and password', 'info');
            return;
        }
        data.user_pwd = (window as any).md5(data.user_pwd);
        let url = window.location.protocol + "//" + (window as any).appConfig.domainApiUrl + (window as any).appConfig.loginReqUrl;
        let reqData = {};
            reqData["account"] = data.user_id;
            reqData["password"] = data.user_pwd;
            reqData["merchant"] = data.merchant_code;;
        HttpClient.postJSON(url , reqData, (json) => {
            if (json) {
                if(json.error_code == 0) { 
                    this.redirect(json);
                } else ($ as any).messager.alert('Login', json.error_message, 'error');
            }
            else ($ as any).messager.alert('Login', 'unknown', 'error');
        });
    }

    redirect(json)
    {
        let href  = (window as any).appConfig.mainPage;
        href += "?merchantCode=" + json.merchant;
        href += "&userId=" + json.account ;
        href += "&sessionId=" + json.session_id ;
        //href += "&isOperator=" + json.isOperator;
        window.location.href = href;
    }

}

export const gui = new Gui();
