using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.AcceptBet
{
    public class BetCollector
    {
        private IServerNode m_Node = null;
        private IServerLogger m_Logger = null;

        private string m_MainCache = "SharpNode";

        private Dictionary<string, decimal> m_PayRates = new Dictionary<string, decimal>()
        {
            {"B", 1.95m},
            {"P", 2.00m},
            {"T", 9.00m}
        };

        //private Dictionary<string, List<long>> m_BetRecordLists = new Dictionary<string, List<long>>();

        public BetCollector(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();
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
    }
}
