using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.FrontEndServer.TableInfo
{
    public class TableInfoDeliverer
    {
        static CommonRng m_Rng = new CommonRng();

        private Timer m_Timer = null;

        private IServerNode m_Node = null;
        private IServerLogger m_Logger = null;

        private bool m_IsRunning = false;

        private string m_MainCache = "MainCache";

        public TableInfoDeliverer(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();

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

        private void Deliver()
        {
            List<dynamic> list = new List<dynamic>();
            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = "select * from tbl_round_state where backup_number = 0 ";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new
                            {
                                server = reader["server_code"].ToString(),
                                table = reader["table_code"].ToString(),
                                shoe = reader["shoe_code"].ToString(),
                                round = reader["round_number"].ToString(),
                                state = reader["round_state"].ToString(),
                                status = reader["round_state_text"].ToString(),
                                countdown = reader["bet_time_countdown"].ToString(),
                                starttime = Convert.ToDateTime(reader["round_start_time"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                updatetime = Convert.ToDateTime(reader["round_update_time"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                history = reader["game_history"].ToString(),
                                player = reader["player_cards"].ToString(),
                                banker = reader["banker_cards"].ToString(),
                                result = reader["game_result"].ToString()
                            };

                            list.Add(item);
                        }
                    }
                }
            }

            try
            {
                var clientMsg = new
                {
                    msg = "table_info",
                    tables = list
                };
                var server = m_Node.GetPublicServer();
                if (server != null && server.IsWorking())
                    server.Broadcast(m_Node.GetJsonHelper().ToJsonString(clientMsg));
            }
            catch (Exception ex)
            {
                Console.WriteLine("FES Broadcast Error - " + ex.ToString());
            }

        }
    }
}
