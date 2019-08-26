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
            if (reqstr.Trim().Length <= 0 || !reqstr.Contains("["))
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            List<string> errIds = new List<string>();
            List<Task> dbTasks = new List<Task>();
            foreach (var item in req)
            {
                string uuid = item.bet_uuid.ToString();
                dbTasks.Add(Task.Run(async () =>
                {
                    string dbErr = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(),
                                            "bet-data", "update-result", m_Node.GetJsonHelper().ToJsonString(item));
                    if (dbErr != "ok") errIds.Add(uuid);
                }));
            }

            Task.WaitAll(dbTasks.ToArray());

            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(errIds));

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

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

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

            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(errIds));

        }
    }
}
