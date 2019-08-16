using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.CheckBet
{
    public class BetChecker
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

        public BetChecker(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();
        }

        public List<dynamic> CheckBetsByGameResult(string gameServer)
        {
            //System.Diagnostics.Debugger.Break();

            m_Logger.Info("CheckBetsByGameResult - " + gameServer);

            List<dynamic> bets = new List<dynamic>();

            Dictionary<long, decimal> payouts = new Dictionary<long, decimal>();
            Dictionary<long, int> results = new Dictionary<long, int>();
            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@server_code", gameServer);

                    cmd.CommandText = " select a.game_result, b.bet_id, b.bet_pool, b.bet_amount from db_mini_baccarat.tbl_round_state a, db_mini_baccarat.tbl_bet_record b "
                                    + " where a.round_state = 9 and b.bet_state = 0 "
                                    + " and a.server_code = @server_code "
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

                if (payouts.Count <= 0) return bets;

                foreach (var item in payouts)
                {
                    dynamic bet = new
                    {
                        bet_id = item.Key,
                        pay_amount = item.Value,
                        game_result = results[item.Key]
                    };

                    bets.Add(bet);
                }

                return bets;
            }

        }
    }
}
