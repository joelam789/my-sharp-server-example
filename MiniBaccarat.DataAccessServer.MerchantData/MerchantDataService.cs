using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.DataAccessServer.MerchantData
{
    [Access(Name = "merchant-data", IsPublic = false)]
    public class MerchantDataService
    {
        MerchantDataCache m_LocalCacheUpdater = null;

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_LocalCacheUpdater == null) m_LocalCacheUpdater = new MerchantDataCache(node);

            await Task.Delay(50);
            if (m_LocalCacheUpdater != null) m_LocalCacheUpdater.Start();
            await Task.Delay(50);

            node.GetLogger().Info(this.GetType().Name + " service started");

            return "";
        }

        [Access(Name = "on-unload", IsLocal = true)]
        public async Task<string> Unload(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(100);
            if (m_LocalCacheUpdater != null)
            {
                m_LocalCacheUpdater.Stop();
                m_LocalCacheUpdater = null;
            }
            await Task.Delay(100);

            return "";
        }

        [Access(Name = "get-merchant-url")]
        public async Task GetMerchantUrlFromLocalCache(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_LocalCacheUpdater == null)
            {
                await ctx.Session.Send("Service not available");
                return;
            }

            string merchantCode = ctx.Data.ToString();
            if (merchantCode.Trim().Length <= 0)
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            var url = m_LocalCacheUpdater.GetMerchantUrl(merchantCode);
            await ctx.Session.Send(url);

        }
    }
}
