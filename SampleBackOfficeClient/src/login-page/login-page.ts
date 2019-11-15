
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

        //this.getConnectionString();
    }

    login() {
        let data = this.agentForm.getData();

        if (!data || data.user_id.length <= 0 || data.user_pwd.length <= 0) {
            ($ as any).messager.alert('Login','Please input user ID and password', 'info');
            return;
        }
        data.user_pwd = (window as any).md5(data.user_pwd);
        let url = (window as any).appConfig.loginReqUrl;
        let data1 = {};
            data1["loginName"] = data.user_id;
            //data1["userPwd"] = data.user_pwd;
            data1["step1"] = 1;
        HttpClient.postJSON(url , data1, (json) => {
            if (json)
            {
                if(json.error == 0) { 
                    let loginName = json.Data.loginName;
                    let salt = json.Data.salt;
                    //this.login2(salt);
                    this.redirect(json);
                }
                else
                if(json.error == -1) { ($ as any).messager.alert('Login', 'invalid input', 'error');}
                else
                if(json.error == -4) { ($ as any).messager.alert('Login', 'disabled', 'error');}
                else
                if(json.error == -5) { ($ as any).messager.alert('Login', 'db error', 'error');}
            }
            else
            {
                ($ as any).messager.alert('Login', 'unknown', 'error');
            }
        });
    }

    redirect(json)
    {
        let href  = (window as any).appConfig.mainPage;
        href += "?UserId2=" + json.Data.Id + "&userId=" + json.Data.LoginId;
        href += "&sessionId=" + json.sessionId ;
        href += "&defaultAgentId=" + json.Data.DefaultAgentId;
        href += "&isOperator=" + json.isOperator;
        window.location.href = href;
    }

    getConnectionString()
    {
        let url = (window as any).appConfig.getConnectionString;
        let data1 = {};
        HttpClient.postJSON(url , data1, (json) => {
            if (json)
            {
                ($('#connectionStringText') as any).html(json);
            }
            else
            {
                ($ as any).messager.alert('Cannot get Connection String');
            }
        });
    }

}

export const gui = new Gui();
