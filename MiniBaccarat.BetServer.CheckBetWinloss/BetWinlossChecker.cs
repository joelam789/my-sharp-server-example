using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.CheckBetWinloss
{
    public class BetWinlossChecker
    {
        static CommonRng m_Rng = new CommonRng();

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

        public BetWinlossChecker(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();
        }

        public List<dynamic> CheckBetWinlossByGameResult(string gameServer)
        {
            //System.Diagnostics.Debugger.Break();

            m_Logger.Info("CheckBetWinlossByGameResult - " + gameServer);

            List<dynamic> bets = new List<dynamic>();

            Dictionary<string, decimal> payouts = new Dictionary<string, decimal>();
            Dictionary<string, int> results = new Dictionary<string, int>();
            Dictionary<string, string> merchants = new Dictionary<string, string>();
            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@server_code", gameServer);

                    cmd.CommandText = " select a.game_result, b.bet_uuid, b.merchant_code, b.bet_pool, b.bet_amount from db_mini_baccarat.tbl_round_state a, db_mini_baccarat.tbl_bet_record b "
                                    + " where a.round_state = 9 and b.bet_state = 0 "
                                    + " and a.server_code = @server_code "
                                    + " and a.server_code = b.server_code "
                                    + " and a.table_code = b.table_code "
                                    + " and a.shoe_code = b.shoe_code "
                                    + " and a.round_number = b.round_number "
                                    ;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string result = reader["game_result"].ToString();
                            string betGuid = reader["bet_uuid"].ToString();
                            string merchant = reader["merchant_code"].ToString();
                            int pool = Convert.ToInt32(reader["bet_pool"].ToString());
                            decimal amount = Convert.ToDecimal(reader["bet_amount"].ToString());

                            if (payouts.ContainsKey(betGuid)) continue;

                            merchants.Add(betGuid, merchant);

                            if (result[0] == '1' && pool == 1) payouts.Add(betGuid, amount * m_PayRates["B"]);
                            else if (result[0] == '2' && pool == 2) payouts.Add(betGuid, amount * m_PayRates["P"]);
                            else if (result[0] == '3' && pool == 3) payouts.Add(betGuid, amount * m_PayRates["T"]);
                            else if (result[0] == '3' && (pool == 1 || pool == 2)) payouts.Add(betGuid, amount); // zero winloss
                            else payouts.Add(betGuid, 0); // lose

                            results.Add(betGuid, Convert.ToInt32(result));

                        }
                    }
                }

                if (payouts.Count <= 0) return bets;

                foreach (var item in payouts)
                {
                    dynamic bet = new
                    {
                        bet_uuid = item.Key,
                        pay_amount = item.Value,
                        game_result = results[item.Key],
                        merchant_code = merchants[item.Key]
                    };

                    bets.Add(bet);
                }

                return bets;
            }

        }
    }
}
