using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.GameServer.Service
{
    [Access(Name = "baccarat")]
    public class GameService
    {
        BaccaratGame m_Game = null;

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            if (m_Game == null) m_Game = new BaccaratGame(node);

            node.GetLogger().Info(this.GetType().Name + " is testing...");
            await Task.Delay(100);

            //var lines = m_Game.PlayOneRound();
            //foreach (var line in lines) node.GetServerLogger().Info(line);

            if (m_Game != null) m_Game.Start();

            await Task.Delay(100);
            node.GetLogger().Info(this.GetType().Name + " test done");

            return "";
        }

        [Access(Name = "on-unload", IsLocal = true)]
        public async Task<string> Unload(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(100);
            if (m_Game != null)
            {
                m_Game.Stop();
                m_Game = null;
            }
            await Task.Delay(100);

            return "";
        }

    }
}
