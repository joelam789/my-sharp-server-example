using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.FrontEndServer.BetResult
{
    public class BetResultDeliverer
    {
        static CommonRng m_Rng = new CommonRng();

        private Timer m_Timer = null;

        private IServerNode m_Node = null;
        private IServerLogger m_Logger = null;

        private bool m_IsRunning = false;

        private string m_MainCache = "MainCache";

        private string m_ServerName = "";

        private ConcurrentDictionary<string, IWebSession> m_Clients = new ConcurrentDictionary<string, IWebSession>();

        public BetResultDeliverer(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();

            m_ServerName = m_Node.GetName();

            m_IsRunning = false;
        }

        public void Start()
        {
            Stop();

            m_Timer = new Timer(Tick, m_Rng, 500, 1000 * 1);
        }

        public void Stop()
        {
            if (m_Timer != null)
            {
                Thread.Sleep(500);
                m_Timer.Dispose();
                m_Timer = null;
            }

            m_Clients.Clear();

        }

        public void AddClient(string clientId, IWebSession client)
        {
            IWebSession oldOne = null;
            if (m_Clients.ContainsKey(clientId)) m_Clients.TryRemove(clientId, out oldOne);
            m_Clients.TryAdd(clientId, client);
        }

        public void RemoveClient(string clientId)
        {
            IWebSession oldOne = null;
            if (m_Clients.ContainsKey(clientId)) m_Clients.TryRemove(clientId, out oldOne);
        }

        private void Tick(object param)
        {
            if (m_IsRunning) return;
            m_IsRunning = true;
            try
            {
                Deliver();
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex.ToString());
                m_Logger.Error(ex.StackTrace);
            }
            finally
            {
                m_IsRunning = false;
            }

        }

        private async void Deliver()
        {
            Dictionary<string, List<dynamic>> betResults = new Dictionary<string, List<dynamic>>();
            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@front_end", m_ServerName);

                    cmd.CommandText = " update tbl_bet_record "
                                    + " set bet_state = 2 " // that means we are going to send them
                                    + " where front_end = @front_end and bet_state = 1 ";

                    cmd.ExecuteNonQuery();
                }

                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@front_end", m_ServerName);

                    // select records which are ready to be sent
                    cmd.CommandText = " select * from tbl_bet_record "
                                    + " where front_end = @front_end and bet_state = 2 ";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new
                            {
                                client = reader["client_id"].ToString(),
                                server = reader["server_code"].ToString(),
                                table = reader["table_code"].ToString(),
                                shoe = reader["shoe_code"].ToString(),
                                round = Convert.ToInt32(reader["round_number"].ToString()),
                                pool = Convert.ToInt32(reader["bet_pool"].ToString()),
                                bet = Convert.ToDecimal(reader["bet_amount"].ToString()),
                                payout = Convert.ToDecimal(reader["pay_amount"].ToString()),
                                result = Convert.ToInt32(reader["game_result"].ToString())
                            };

                            if (betResults.ContainsKey(item.client))
                            {
                                var list = betResults[item.client];
                                list.Add(item);
                            }
                            else
                            {
                                var list = new List<dynamic>();
                                list.Add(item);
                                betResults.Add(item.client, list);
                            }
                            
                        }
                    }
                }

                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@front_end", m_ServerName);

                    // remove them
                    cmd.CommandText = " delete from tbl_bet_record "
                                    + " where front_end = @front_end and bet_state = 2 ";

                    cmd.ExecuteNonQuery();
                }
            }

            foreach (var item in betResults)
            {
                try
                {
                    var list = item.Value;
                    var clientMsg = new
                    {
                        msg = "bet_result",
                        results = list
                    };
                    IWebSession client = null;
                    if (m_Clients.TryGetValue(item.Key, out client))
                    {
                        await client.Send(m_Node.GetJsonHelper().ToJsonString(clientMsg));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FES Send-Bet-Result Error - " + ex.ToString());
                }
            }

            if (betResults.Count > 0) m_Logger.Info("Sent bet results - " + betResults.Count);
        }
    }
}
