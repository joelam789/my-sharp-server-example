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

        public async Task<dynamic> AcceptBet(dynamic betreq)
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

            string betGuid = "";
            decimal playerBalance = -1;

            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@server_code", betreq.server_code);
                    dbhelper.AddParam(cmd, "@table_code", betreq.table_code);
                    dbhelper.AddParam(cmd, "@shoe_code", betreq.shoe_code);
                    dbhelper.AddParam(cmd, "@round_number", betreq.round_number);

                    cmd.CommandText = " select * from db_mini_baccarat.tbl_round_state "
                                    + " where round_state = 4 and bet_time_countdown > 0 "
                                    + " and server_code = @server_code and table_code = @table_code "
                                    + " and shoe_code = @shoe_code and round_number = @round_number ";
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
                    m_Logger.Info("Saving bet record to database...");

                    string betTime = "";
                    string retStr = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(),
                                 "bet-data", "save-record", m_Node.GetJsonHelper().ToJsonString(betreq));

                    if (retStr.Contains("{") && retStr.Contains("-"))
                    {
                        dynamic ret = m_Node.GetJsonHelper().ToJsonObject(retStr);
                        betGuid = ret.bet_uuid;
                        betTime = ret.bet_time;
                        m_Logger.Info("Update database successfully");
                    }
                    else
                    {
                        m_Logger.Error("Failed to save bet data in database");
                    }

                    if (betGuid.Length > 0 && betTime.Length > 0)
                    {
                        // call single wallet

                        m_Logger.Info("Call single wallet...");

                        var swReq = new
                        {
                            bet_uuid = betGuid,
                            table_code = betreq.table_code,
                            shoe_code = betreq.shoe_code,
                            round_number = betreq.round_number,
                            bet_pool = betreq.bet_pool,
                            merchant_code = betreq.merchant_code,
                            player_id = betreq.player_id,
                            bet_amount = betreq.bet_amount,
                            bet_time = betTime
                        };

                        string swReplyStr = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(),
                                 "single-wallet", "debit-for-placing-bet", m_Node.GetJsonHelper().ToJsonString(swReq));

                        if (String.IsNullOrEmpty(swReplyStr))
                        {
                            replyErrorCode = -5;
                            replyErroMsg = "failed to call single-wallet service";
                        }
                        else
                        {
                            dynamic ret = m_Node.GetJsonHelper().ToJsonObject(swReplyStr);

                            if (ret.error_code == 0)
                            {
                                playerBalance = ret.player_balance;
                            }
                            else
                            {
                                replyErrorCode = -5;
                                replyErroMsg = "failed to debit from merchant";
                            }
                        }
                    }
                    else
                    {
                        replyErrorCode = -4;
                        replyErroMsg = "failed to add it to db";
                    }

                    if (replyErrorCode >= 0 && playerBalance >= 0)
                    {
                        using (var cmd = cnn.CreateCommand())
                        {
                            dbhelper.AddParam(cmd, "@bet_uuid", betGuid);

                            dbhelper.AddParam(cmd, "@merchant_code", betreq.merchant_code);
                            dbhelper.AddParam(cmd, "@player_id", betreq.player_id);

                            dbhelper.AddParam(cmd, "@server_code", betreq.server_code);
                            dbhelper.AddParam(cmd, "@table_code", betreq.table_code);
                            dbhelper.AddParam(cmd, "@shoe_code", betreq.shoe_code);
                            dbhelper.AddParam(cmd, "@round_number", betreq.round_number);
                            dbhelper.AddParam(cmd, "@client_id", betreq.client_id);
                            dbhelper.AddParam(cmd, "@front_end", betreq.front_end);
                            dbhelper.AddParam(cmd, "@bet_pool", betreq.bet_pool);
                            dbhelper.AddParam(cmd, "@bet_amount", betreq.bet_amount);

                            cmd.CommandText = " insert into db_mini_baccarat.tbl_bet_record "
                                            + " ( bet_uuid, merchant_code, player_id, server_code, table_code, shoe_code, round_number, client_id, front_end, bet_pool, bet_amount, bet_time ) values "
                                            + " ( @bet_uuid, @merchant_code, @player_id, @server_code , @table_code , @shoe_code , @round_number , @client_id , @front_end , @bet_pool, @bet_amount , CURRENT_TIMESTAMP ) "
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
                }

                if (replyErrorCode >= 0)
                {
                    return new
                    {
                        msg = replyMsgType,
                        player_balance = playerBalance,
                        error_code = 0,
                        error_msg = "ok"
                    };
                }
                else return new
                {
                    msg = replyMsgType,
                    player_balance = playerBalance,
                    error_code = replyErrorCode,
                    error_msg = replyErroMsg
                };
            }
        }
    }
}
