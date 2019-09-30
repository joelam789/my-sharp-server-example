using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.SampleMerchant.Api
{
    [Access(Name = "player")]
    public class PlayerService
    {
        [Access(Name = "validate-login")]
        public async Task ValidateLogin(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send( " { \"error_code\": -1, \"error_message\": \"Invalid request\" } " );
                return;
            }

            decimal balance = -1;
            bool found = false;

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            string merchantCode = req.merchant_code.ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                    dbhelper.AddParam(cmd, "@player_id", req.player_id);
                    dbhelper.AddParam(cmd, "@session_id", req.login_token);

                    cmd.CommandText = "select * from tbl_player_balance "
                                        + " where merchant_code = @merchant_code "
                                        + " and player_id = @player_id "
                                        ;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            balance = Convert.ToDecimal(reader["player_balance"].ToString());
                            found = true;
                        }
                    }
                }

                if (!found)
                {
                    balance = 900000; // default balance value for demo

                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@player_balance", balance);

                        cmd.CommandText = "insert into tbl_player_balance "
                                            + " ( merchant_code, player_id, player_balance, update_time ) values "
                                            + " ( @merchant_code , @player_id , @player_balance , NOW() ) "
                                            ;

                        var okay = cmd.ExecuteNonQuery() > 0;

                        if (!okay) balance = -1;
                    }
                }
            }

            var reply = new
            {
                req.merchant_code,
                req.player_id,
                player_balance = balance,
                error_code = balance >= 0 ? 0 : -1
            };
            if (reply.error_code == 0) ctx.Logger.Info("Three-Way Login Passed on merchant site!");
            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(reply));
        }

        [Access(Name = "get-player-balance")]
        public async Task GetPlayerBalance(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send(" { \"error_code\": -1, \"error_message\": \"Invalid request\" } ");
                return;
            }

            decimal balance = 0;
            bool found = false;

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            string merchantCode = req.merchant_code.ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                    dbhelper.AddParam(cmd, "@player_id", req.player_id);
                    dbhelper.AddParam(cmd, "@session_id", req.login_token);

                    cmd.CommandText = "select * from tbl_player_balance "
                                        + " where merchant_code = @merchant_code "
                                        + " and player_id = @player_id "
                                        ;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            balance = Convert.ToDecimal(reader["player_balance"].ToString());
                            found = true;
                        }
                    }
                }

                if (!found) balance = 0;
            }

            var reply = new
            {
                req.merchant_code,
                req.player_id,
                player_balance = balance,
                error_code = balance >= 0 ? 0 : -1
            };
            
            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(reply));
        }
    }
}
