using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.AcceptBet
{
    [Access(Name = "accept-bet")]
    public class AcceptBetService
    {
        IServerNode m_Node = null;
        BetCollector m_BetCollector = null;

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            m_Node = node;

            if (m_BetCollector == null) m_BetCollector = new BetCollector(node);

            await Task.Delay(50);

            m_Node.GetLogger().Info(this.GetType().Name + " service started");

            return "";
        }

        [Access(Name = "on-unload", IsLocal = true)]
        public async Task<string> Unload(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(100);
            if (m_BetCollector != null)
            {
                m_BetCollector = null;
            }
            await Task.Delay(100);

            return "";
        }

        [Access(Name = "accept")]
        public async Task AcceptBet(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_BetCollector == null)
            {
                await ctx.Session.Send("Service not available");
                return;
            }

            string betstr = ctx.Data.ToString();
            if (betstr.Trim().Length <= 0)
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            dynamic betreq = ctx.JsonCodec.ToJsonObject(betstr);

            var reply = await m_BetCollector.AcceptBet(betreq);

            if (reply.error_code == 0)
                ctx.Logger.Info("BET - [" + betreq.merchant_code + " - "  + betreq.player_id + "], " 
                    + "[" + betreq.round_number + "], " + betreq.bet_pool + " , " + betreq.bet_amount);

            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(reply));

        }
    }
}
