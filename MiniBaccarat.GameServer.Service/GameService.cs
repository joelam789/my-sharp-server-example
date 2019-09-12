using System;
using System.Collections.Generic;
using System.Configuration;
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

            node.GetLogger().Info(this.GetType().Name + " is loading settings from config...");
            
            //var lines = m_Game.PlayOneRound();
            //foreach (var line in lines) node.GetServerLogger().Info(line);

            try
            {
                ConfigurationManager.RefreshSection("appSettings");
                await Task.Delay(100);
                var keys = ConfigurationManager.AppSettings.Keys;
                foreach (var key in keys)
                {
                    if (key.ToString() == "GameTable")
                    {
                        m_Game.TableCode = ConfigurationManager.AppSettings["GameTable"].ToString();
                        break;
                    }
                }
                node.GetLogger().Info("Done");
            }
            catch(Exception ex)
            {
                node.GetLogger().Error("Failed to load settings from config for BaccaratGame: ");
                node.GetLogger().Error(ex.ToString());
            }

            if (m_Game != null) m_Game.Start();
            await Task.Delay(100);

            node.GetLogger().Info(this.GetType().Name + " started");

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
