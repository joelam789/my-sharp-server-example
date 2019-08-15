using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.Betting
{
    [Access(Name = "betting")]
    public class BetService
    {
        BetChecker m_BetCheck = null;

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_BetCheck == null) m_BetCheck = new BetChecker(node);

            await Task.Delay(50);
            if (m_BetCheck != null) m_BetCheck.Start();
            await Task.Delay(50);

            node.GetLogger().Info(this.GetType().Name + " service started");

            return "";
        }

        [Access(Name = "on-unload", IsLocal = true)]
        public async Task<string> Unload(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(100);
            if (m_BetCheck != null)
            {
                m_BetCheck.Stop();
                m_BetCheck = null;
            }
            await Task.Delay(100);

            return "";
        }

        [Access(Name = "place-bet")]
        public async Task PlaceBet(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_BetCheck == null)
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

            var reply = m_BetCheck.AcceptBet(betreq);
            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(reply));

        }
    }
}
