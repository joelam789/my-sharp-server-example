
import { HttpClient } from "../http-client";
import { BaseTree } from "../ui-common/base-tree";

export class MenuTree extends BaseTree {

    init(callback?) {
        super.init(() => {
            if (this.gui) {
                var data = [];
                data.push({
                    id: 1,
                    text: "代理管理",
                    iconCls: "icon-agents",
                    attributes: {
                        url: (window as any).appConfig.infoPage
                    }
                });
                data.push({
                    id: 2,
                    text: "代理报表",
                    iconCls: "icon-report",
                    attributes: {
                        url: (window as any).appConfig.reportPage
                    }
                });
                data.push({
                    id: 3,
                    text: "代理结算",
                    iconCls: "icon-sum",
                    attributes: {
                        url: (window as any).appConfig.agentSettlePage
                    }
                });
                /*
                */
                data.push({
                    id: 4,
                    text: "结算记录",
                    iconCls: "icon-sum",
                    attributes: {
                        url: (window as any).appConfig.SettlementLog
                    }
                });
                data.push({
                    id: 5,
                    text: "调整记录",
                    iconCls: "icon-sum",
                    attributes: {
                        url: (window as any).appConfig.adjustmentLogPage
                    }
                });
                if ( (window as any).appConfig.isOperator == "Operator" ) {
                    data.push(
                        {
                            id: 6,
                            text: "汇入资料",
                            iconCls: "icon-filter",
                            attributes: {
                                url: (window as any).appConfig.importStatisticPage
                            }
                        }
                    );
                }
                data.push({
                    id: 7,
                    text: "玩家管理",
                    iconCls: "icon-sum",
                    attributes: {
                        url: (window as any).appConfig.playerMgtPage
                    }
                });
                data.push({
                    id: 8,
                    text: "玩家输嬴",
                    iconCls: "icon-sum",
                    attributes: {
                        url: (window as any).appConfig.playerWinLossPage
                    }
                });
                data.push({
                    id: 8,
                    text: "修改密码",
                    iconCls: "icon-lock",
                    attributes: {
                        func: (treeNode) => {
                            (window as any).gui.openPasswordDialog();
                        }
                    }
                });
                data.push({
                    id: 9,
                    text: "系统登出",
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
                            url += "?UserId2=" + (window as any).appConfig.UserId2;
                            url += "&userId=" + (window as any).appConfig.userId;
                            url += "&sessionId=" + (window as any).appConfig.sessionId;
                            url += "&BetDate=" + (window as any).appConfig.BetDate;
                            url += "&approvedBetDate=" + (window as any).appConfig.approvedBetDate;
                            url += "&settlementDate=" + (window as any).appConfig.settlementDate;
                            url += "&defaultAgentId=" + (window as any).appConfig.defaultAgentId;
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

