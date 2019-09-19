using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.CheckBetWinloss
{
    [Access(Name = "check-bet-winloss")]
    public class CheckBetWinlossService
    {
        BetWinlossChecker m_Checker = null;

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_Checker == null) m_Checker = new BetWinlossChecker(node);

            await Task.Delay(50);

            node.GetLogger().Info(this.GetType().Name + " service started");

            return "";
        }

        [Access(Name = "on-unload", IsLocal = true)]
        public async Task<string> Unload(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(100);
            if (m_Checker != null)
            {
                m_Checker = null;
            }
            await Task.Delay(100);

            return "";
        }

        [Access(Name = "check-and-update")]
        public async Task CheckAndUpdate(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_Checker == null)
            {
                await ctx.Session.Send("Service not available");
                return;
            }

            string gameServerName = ctx.Data.ToString();
            if (gameServerName.Trim().Length <= 0)
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            var reply = m_Checker.CheckBetWinlossByGameResult(gameServerName);
            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(reply));

        }
    }
}
