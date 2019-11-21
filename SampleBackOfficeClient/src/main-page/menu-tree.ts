
import { HttpClient } from "../http-client";
import { BaseTree } from "../ui-common/base-tree";

export class MenuTree extends BaseTree {

    init(callback?) {
        super.init(() => {
            if (this.gui) {
                let data = [];
                data.push({
                    id: 1,
                    text: "Game Results",
                    iconCls: "icon-report",
                    attributes: {
                        url: (window as any).appConfig.gameResultSearchPage
                    }
                });
                data.push({
                    id: 2,
                    text: "Bet Records",
                    iconCls: "icon-report",
                    attributes: {
                        url: (window as any).appConfig.reportPage
                    }
                });
                data.push({
                    id: 8,
                    text: "Change Password",
                    iconCls: "icon-lock",
                    attributes: {
                        func: (treeNode) => {
                            (window as any).gui.openPasswordDialog();
                        }
                    }
                });
                data.push({
                    id: 9,
                    text: "Logout",
                    iconCls: "icon-back",
                    attributes: {
                        func: (treeNode) => {
                            HttpClient.getJSON((window as any).appConfig.logoutReqUrl, null, (json) => {
                                if (json && json.error === 0) console.log("logged out");
                                window.location.href = (window as any).appConfig.loginPage;
                            });
                        }
                    }
                });

                this.gui({
                    animate: true,
                    data:data,
                    cache: false,
                    state: closed,
                    onClick: (node) => {
                        let url = node.attributes.url;
                        if (url) {
                            url += "?merchantCode=" + (window as any).appConfig.merchantCode;
                            url += "&userId=" + (window as any).appConfig.userId;
                            url += "&sessionId=" + (window as any).appConfig.sessionId;
                            (window as any).gui.mainPanel.go(url);
                        } else {
                            let func = node.attributes.func;
                            if (func) func(node);
                        }
                    }
                });
            }
            if (callback) callback();
        });
    }
}

