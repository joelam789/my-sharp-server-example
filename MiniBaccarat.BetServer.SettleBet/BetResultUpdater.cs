﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BetServer.SettleBet
{
    public class BetResultUpdater
    {
        private IServerNode m_Node = null;
        private IServerLogger m_Logger = null;

        private string m_MainCache = "SharpNode";

        public BetResultUpdater(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();
        }

        public void Update(dynamic bets)
        {
            //System.Diagnostics.Debugger.Break();

            m_Logger.Info("Settle bets - " + bets.Count);

            if (bets.Count <= 0) return;
            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                foreach (var bet in bets)
                {
                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@bet_id", bet.bet_id);
                        dbhelper.AddParam(cmd, "@pay_amount", bet.pay_amount);
                        dbhelper.AddParam(cmd, "@game_result", bet.game_result);

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

            m_Logger.Info("done");

        }
    }
}
