using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.Betting
{
    public class BetChecker
    {
        static CommonRng m_Rng = new CommonRng();

        private Timer m_Timer = null;

        private IServerNode m_Node = null;
        private IServerLogger m_Logger = null;

        private bool m_IsRunning = false;

        private string m_MainCache = "SharpNode";

        private Dictionary<string, decimal> m_PayRates = new Dictionary<string, decimal>()
        {
            {"B", 1.95m},
            {"P", 2.00m},
            {"T", 9.00m}
        };

        //private Dictionary<string, List<long>> m_BetRecordLists = new Dictionary<string, List<long>>();

        public BetChecker(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();

            m_IsRunning = false;
        }

        public void Start()
        {
            Stop();
            //lock (m_BetRecordLists) m_BetRecordLists.Clear();
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

        public dynamic AcceptBet(dynamic betreq)
        {
            string replyMsgType = "bet_reply";
            int replyErrorCode = -1;
            string replyErroMsg = "input invalid";

            if (betreq.bet_pool < 1)
            {
                return new
                {
                    msg = replyMsgType,

                    error_code = replyErrorCode,
                    error_msg = replyErroMsg
                };
            }

            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@server_code", betreq.server_code);
                    dbhelper.AddParam(cmd, "@game_code", betreq.game_code);
                    dbhelper.AddParam(cmd, "@round_number", betreq.round_number);

                    cmd.CommandText = " select * from db_mini_baccarat.tbl_round_state "
                                    + " where round_state = 4 and bet_time_countdown > 0 "
                                    + " and server_code = @server_code and game_code = @game_code and round_number = @round_number ";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            replyErrorCode = 2;
                            replyErroMsg = "timing is fine";
                        }
                        else
                        {
                            replyErrorCode = -2;
                            replyErroMsg = "out of betting time";
                        }
                    }
                }

                if (replyErrorCode >= 0)
                {
                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@server_code", betreq.server_code);
                        dbhelper.AddParam(cmd, "@game_code", betreq.game_code);
                        dbhelper.AddParam(cmd, "@round_number", betreq.round_number);
                        dbhelper.AddParam(cmd, "@client_id", betreq.client_id);
                        dbhelper.AddParam(cmd, "@front_end", betreq.front_end);
                        dbhelper.AddParam(cmd, "@bet_pool", betreq.bet_pool);
                        dbhelper.AddParam(cmd, "@bet_amount", betreq.bet_amount);

                        cmd.CommandText = " insert into db_mini_baccarat.tbl_bet_record "
                                        + " ( server_code, game_code, round_number, client_id, front_end, bet_pool, bet_amount, bet_time ) values "
                                        + " ( @server_code , @game_code , @round_number , @client_id , @front_end , @bet_pool, @bet_amount , CURRENT_TIMESTAMP ) "
                                        ;

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            replyErrorCode = 3;
                            replyErroMsg = "added to cache";
                        }
                        else
                        {
                            replyErrorCode = -3;
                            replyErroMsg = "failed to add it to cache";
                        }

                    }
                }

                if (replyErrorCode >= 0)
                {
                    return new
                    {
                        msg = replyMsgType,

                        error_code = 0,
                        error_msg = "ok"
                    };
                }
                else return new
                {
                    msg = replyMsgType,

                    error_code = replyErrorCode,
                    error_msg = replyErroMsg
                };
            }
        }

        private void Tick(object param)
        {
            if (m_IsRunning) return;
            m_IsRunning = true;
            try
            {
                Check();
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

        private void Check()
        {
            Dictionary<long, decimal> payouts = new Dictionary<long, decimal>();
            Dictionary<long, int> results = new Dictionary<long, int>();
            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = " select a.game_result, b.bet_id, b.bet_pool, b.bet_amount from db_mini_baccarat.tbl_round_state a, db_mini_baccarat.tbl_bet_record b "
                                    + " where a.round_state = 9 and b.bet_state = 0 "
                                    + " and a.server_code = b.server_code "
                                    + " and a.game_code = b.game_code "
                                    + " and a.round_number = b.round_number "
                                    ;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string result = reader["game_result"].ToString();
                            long betId = Convert.ToInt64(reader["bet_id"].ToString());
                            int pool = Convert.ToInt32(reader["bet_pool"].ToString());
                            decimal amount = Convert.ToDecimal(reader["bet_amount"].ToString());

                            if (payouts.ContainsKey(betId)) continue;

                            if (result[0] == '1' && pool == 1) payouts.Add(betId, amount * m_PayRates["B"]);
                            else if (result[0] == '2' && pool == 2) payouts.Add(betId, amount * m_PayRates["P"]);
                            else if (result[0] == '3' && pool == 3) payouts.Add(betId, amount * m_PayRates["T"]);
                            else payouts.Add(betId, 0); // lose

                            results.Add(betId, Convert.ToInt32(result));
                        }
                    }
                }

                if (payouts.Count <= 0) return;

                foreach (var item in payouts)
                {
                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@bet_id", item.Key);
                        dbhelper.AddParam(cmd, "@pay_amount", item.Value);
                        dbhelper.AddParam(cmd, "@game_result", results[item.Key]);

                        cmd.CommandText = "update db_mini_baccarat.tbl_bet_record "
                                            + " set pay_amount = @pay_amount "
                                            + " , game_result = @game_result "
                                            + " , bet_state = 1 "
                                            + " , settle_time = CURRENT_TIMESTAMP "
                                            + " where bet_id = @bet_id "
                                            ;
                        cmd.ExecuteNonQuery();
                    }
                }
            }

        }
    }
}
