using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.SettleBet
{
    [Access(Name = "settle-bet")]
    public class SettleBetService
    {
        BetResultUpdater m_Updater = null;

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_Updater == null) m_Updater = new BetResultUpdater(node);

            await Task.Delay(50);

            node.GetLogger().Info(this.GetType().Name + " service started");

            return "";
        }

        [Access(Name = "on-unload", IsLocal = true)]
        public async Task<string> Unload(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(100);
            if (m_Updater != null)
            {
                m_Updater = null;
            }
            await Task.Delay(100);

            return "";
        }

        [Access(Name = "settle")]
        public async Task Settle(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_Updater == null)
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

            dynamic betreq = ctx.JsonHelper.ToJsonObject(betstr);

            bool okay = m_Updater.Update(betreq) > 0;

            if (okay) await ctx.Session.Send("ok");
            else await ctx.Session.Send("Failed to update cache");

        }
    }
}
