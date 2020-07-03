using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BatchProcessServer.BatchUpdate
{
    [Access(Name = "batch-update", IsPublic = false)]
    public class BatchUpdateService
    {
        IServerNode m_Node = null;

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(50);
            m_Node = node;
            await Task.Delay(50);

            node.GetLogger().Info(this.GetType().Name + " service started");

            return "";
        }

        [Access(Name = "update-bet-db")]
        public async Task UpdateBetsInDatabase(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0 || !reqstr.Contains('['))
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);

            List<dynamic> passIds = new List<dynamic>();
            List<Task> dbTasks = new List<Task>();
            foreach (var item in req)
            {
                string uuid = item.bet_uuid.ToString();
                dbTasks.Add(Task.Run(async () =>
                {
                    string dbReply = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(),
                                            "bet-data", "update-result", m_Node.GetJsonHelper().ToJsonString(item));
                    //if (dbErr != "ok") errIds.Add(uuid);
                    if (dbReply.Contains('-') && dbReply.Contains('='))
                    {
                        var itemParts = dbReply.Split('=');
                        passIds.Add(new
                        {
                            bet_uuid = itemParts[0],
                            settle_time = itemParts[1]
                        });
                    }
                }));
            }

            Task.WaitAll(dbTasks.ToArray());

            await ctx.Session.Send(ctx.JsonHelper.ToJsonString(passIds));

        }

        [Access(Name = "update-bet-cache")]
        public async Task UpdateBetsInCache(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0 || !reqstr.Contains("["))
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);

            List<string> errIds = new List<string>();
            List<Task> cacheTasks = new List<Task>();
            foreach (var item in req)
            {
                string uuid = item.bet_uuid.ToString();
                cacheTasks.Add(Task.Run(async () =>
                {
                    string cacheErr = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(),
                                                "settle-bet", "settle", m_Node.GetJsonHelper().ToJsonString(item));
                    if (cacheErr != "ok") errIds.Add(uuid);
                }));
            }

            Task.WaitAll(cacheTasks.ToArray());

            await ctx.Session.Send(ctx.JsonHelper.ToJsonString(errIds));

        }

        [Access(Name = "update-bet-wallet")]
        public async Task UpdateSingleWalletForBets(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0 || !reqstr.Contains("["))
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);

            List<string> errIds = new List<string>();
            List<Task> walletTasks = new List<Task>();
            foreach (var item in req)
            {
                string uuid = item.bet_uuid.ToString();
                walletTasks.Add(Task.Run(async () =>
                {
                    string walletReplyStr = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(),
                                                "single-wallet", "credit-for-settling-bet", m_Node.GetJsonHelper().ToJsonString(item));
                    if (String.IsNullOrEmpty(walletReplyStr) || !walletReplyStr.Contains('{'))
                    {
                        errIds.Add(uuid);
                    }
                    else
                    {
                        dynamic walletReply = ctx.JsonHelper.ToJsonObject(walletReplyStr);
                        if (walletReply.error_code != 0) errIds.Add(uuid);
                    }
                    
                }));
            }

            Task.WaitAll(walletTasks.ToArray());

            await ctx.Session.Send(ctx.JsonHelper.ToJsonString(errIds));

        }
    }
}
