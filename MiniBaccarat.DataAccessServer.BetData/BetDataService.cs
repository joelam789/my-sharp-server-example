using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.DataAccessServer.BetData
{
    [Access(Name = "bet-data", IsPublic = false)]
    public class BetDataService
    {
        string m_BaseBetCode = "";
        int m_BetIndex = 0;

        private string GetBetId()
        {
            if (String.IsNullOrEmpty(m_BaseBetCode))
                m_BaseBetCode = Guid.NewGuid().ToString();

            Interlocked.Increment(ref m_BetIndex);

            return m_BaseBetCode + "-" + m_BetIndex;
        }

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            m_BaseBetCode = Guid.NewGuid().ToString();

            node.GetLogger().Info("Reloading database settings from config...");

            await Task.Delay(50);
            node.GetDataHelper().RefreshDatabaseSettings();
            await Task.Delay(50);

            node.GetLogger().Info(this.GetType().Name + " service started");

            return "";
        }

        [Access(Name = "save-record")]
        public async Task SaveRecord(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            bool okay = false;
            string betId = GetBetId();

            dynamic betreq = ctx.JsonCodec.ToJsonObject(reqstr);

            string merchantCode = betreq.merchant_code.ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@bet_uuid", betId);

                    dbhelper.AddParam(cmd, "@merchant_code", betreq.merchant_code);
                    dbhelper.AddParam(cmd, "@player_id", betreq.player_id);

                    dbhelper.AddParam(cmd, "@server_code", betreq.server_code);
                    dbhelper.AddParam(cmd, "@game_code", betreq.game_code);
                    dbhelper.AddParam(cmd, "@round_number", betreq.round_number);
                    dbhelper.AddParam(cmd, "@client_id", betreq.client_id);
                    dbhelper.AddParam(cmd, "@front_end", betreq.front_end);
                    dbhelper.AddParam(cmd, "@bet_pool", betreq.bet_pool);
                    dbhelper.AddParam(cmd, "@bet_amount", betreq.bet_amount);

                    cmd.CommandText = " insert into tbl_bet_record "
                                    + " ( bet_uuid, merchant_code, player_id, server_code, game_code, round_number, client_id, front_end, bet_pool, bet_amount, bet_time ) values "
                                    + " ( @bet_uuid, @merchant_code, @player_id, @server_code , @game_code , @round_number , @client_id , @front_end , @bet_pool, @bet_amount , CURRENT_TIMESTAMP ) "
                                    ;

                    okay = cmd.ExecuteNonQuery() > 0;
                }
            }

            if (okay) await ctx.Session.Send(betId);
            else await ctx.Session.Send("Failed to update database");
        }

        [Access(Name = "update-result")]
        public async Task UpdateResult(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            bool okay = false;

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            string merchantCode = req.merchant_code.ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                    dbhelper.AddParam(cmd, "@pay_amount", req.pay_amount);
                    dbhelper.AddParam(cmd, "@game_result", req.game_result);

                    cmd.CommandText = "update tbl_bet_record "
                                        + " set pay_amount = @pay_amount "
                                        + " , game_result = @game_result "
                                        + " , bet_state = 1 "
                                        + " , update_time = CURRENT_TIMESTAMP "
                                        + " where bet_uuid = @bet_uuid "
                                        ;

                    okay = cmd.ExecuteNonQuery() > 0;
                }
            }

            if (okay) await ctx.Session.Send("ok");
            else await ctx.Session.Send("Failed to update database");
        }
    }
}
