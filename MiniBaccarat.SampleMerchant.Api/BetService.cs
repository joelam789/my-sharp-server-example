using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.SampleMerchant.Api
{
    [Access(Name = "bet")]
    public class BetService
    {
        [Access(Name = "debit-for-placing-bet")]
        public async Task DebitForBetting(RequestContext ctx)
        {
            ctx.Logger.Info("Debit for placing-bet...");

            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Invalid request"
                }));
                return;
            }

            dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);
            bool requestToCancel = req.is_cancelled;

            if (requestToCancel)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = -2,
                    error_message = "Not support cancelling"
                }));
                return;
            }

            var done = false;
            var found = false;
            decimal balance = 0;
            string merchantCode = req.merchant_code.ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@debit_uuid", req.debit_uuid);

                    cmd.CommandText = "select * from tbl_trans_debit "
                                        + " where debit_uuid = @debit_uuid "
                                        ;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            found = true;
                            var success = Convert.ToInt32(reader["debit_success"].ToString());
                            if (success > 0) done = true;
                        }
                    }

                }

                if (found && !done)
                {
                    var trans = cnn.BeginTransaction();

                    using (var cmd = cnn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@debit_amount", req.debit_amount);

                        cmd.CommandText = "update tbl_player_balance "
                                            + " set player_balance = player_balance - @debit_amount "
                                            + " where merchant_code = @merchant_code "
                                            + " and player_id = @player_id "
                                            + " and player_balance >= @debit_amount "
                                            ;

                        done = cmd.ExecuteNonQuery() > 0;
                    }

                    if (done)
                    {
                        using (var cmd = cnn.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            dbhelper.AddParam(cmd, "@debit_uuid", req.debit_uuid);
                            dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                            dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                            dbhelper.AddParam(cmd, "@player_id", req.player_id);
                            dbhelper.AddParam(cmd, "@round_id", req.round_id);
                            dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);
                            dbhelper.AddParam(cmd, "@debit_amount", req.debit_amount);
                            dbhelper.AddParam(cmd, "@bet_time", req.bet_time);
                            dbhelper.AddParam(cmd, "@debit_success", done ? 1 : 0);

                            cmd.CommandText = "update tbl_trans_debit "
                                            + " set bet_uuid = @bet_uuid , merchant_code = @merchant_code , player_id = @player_id ,"
                                            + " round_id = @round_id , bet_pool = @bet_pool , debit_amount = @debit_amount , "
                                            + " debit_success = @debit_success, bet_time = @bet_time , update_time = NOW() "
                                            + " where debit_uuid = @debit_uuid "
                                            ;

                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = cnn.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            dbhelper.AddParam(cmd, "@debit_uuid", req.debit_uuid);
                            dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                            dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                            dbhelper.AddParam(cmd, "@player_id", req.player_id);
                            dbhelper.AddParam(cmd, "@round_id", req.round_id);
                            dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);
                            dbhelper.AddParam(cmd, "@debit_amount", req.debit_amount);
                            dbhelper.AddParam(cmd, "@bet_time", req.bet_time);

                            cmd.CommandText = "update tbl_bet_record "
                                            + " set debit_uuid = @debit_uuid , merchant_code = @merchant_code , player_id = @player_id ,"
                                            + " round_id = @round_id , bet_pool = @bet_pool , bet_amount = @debit_amount , "
                                            + " bet_time = @bet_time , update_time = NOW() "
                                            + " where bet_uuid = @bet_uuid "
                                            ;

                            cmd.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                }
                else if (!found && !done)
                {
                    var trans = cnn.BeginTransaction();

                    using (var cmd = cnn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@debit_amount", req.debit_amount);

                        cmd.CommandText = "update tbl_player_balance "
                                            + " set player_balance = player_balance - @debit_amount "
                                            + " where merchant_code = @merchant_code "
                                            + " and player_id = @player_id "
                                            + " and player_balance >= @debit_amount "
                                            ;

                        done = cmd.ExecuteNonQuery() > 0;
                    }

                    using (var cmd = cnn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        dbhelper.AddParam(cmd, "@debit_uuid", req.debit_uuid);
                        dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@round_id", req.round_id);
                        dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);
                        dbhelper.AddParam(cmd, "@debit_amount", req.debit_amount);
                        dbhelper.AddParam(cmd, "@bet_time", req.bet_time);
                        dbhelper.AddParam(cmd, "@debit_success", done ? 1 : 0);

                        cmd.CommandText = "insert into tbl_trans_debit "
                                        + " set debit_uuid = @debit_uuid , bet_uuid = @bet_uuid , merchant_code = @merchant_code , player_id = @player_id ,"
                                        + " round_id = @round_id , bet_pool = @bet_pool , debit_amount = @debit_amount , "
                                        + " debit_success = @debit_success, bet_time = @bet_time , update_time = NOW() "
                                        ;

                        found = cmd.ExecuteNonQuery() > 0;
                    }

                    using (var cmd = cnn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        dbhelper.AddParam(cmd, "@debit_uuid", req.debit_uuid);
                        dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@round_id", req.round_id);
                        dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);
                        dbhelper.AddParam(cmd, "@debit_amount", done ? req.debit_amount : 0);
                        dbhelper.AddParam(cmd, "@bet_time", req.bet_time);

                        cmd.CommandText = "insert into tbl_bet_record "
                                        + " set bet_uuid = @bet_uuid, debit_uuid = @debit_uuid , merchant_code = @merchant_code , player_id = @player_id ,"
                                        + " round_id = @round_id , bet_pool = @bet_pool , bet_amount = @debit_amount , "
                                        + " bet_time = @bet_time , update_time = NOW() "
                                        ;

                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                }

                if (found && done)
                {
                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);

                        cmd.CommandText = "select * from tbl_player_balance "
                                            + " where merchant_code = @merchant_code "
                                            + " and player_id = @player_id "
                                            ;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                balance = Convert.ToDecimal(reader["player_balance"].ToString());
                            }
                        }
                    }
                }
            }

            ctx.Logger.Info("Debit done");

            var reply = new
            {
                req.merchant_code,
                req.player_id,
                player_balance = balance,
                error_code = found && done ? 0 : -1
            };
            await ctx.Session.Send(ctx.JsonHelper.ToJsonString(reply));
        }


        [Access(Name = "credit-for-settling-bet")]
        public async Task CreditForBetting(RequestContext ctx)
        {
            ctx.Logger.Info("Credit for settling-bet...");

            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Invalid request"
                }));
                return;
            }

            dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);
            bool requestToCancel = req.is_cancelled;

            if (requestToCancel)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = -2,
                    error_message = "Not support cancelling"
                }));
                return;
            }

            var done = false;
            var found = false;
            decimal balance = 0;
            string merchantCode = req.merchant_code.ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@credit_uuid", req.credit_uuid);

                    cmd.CommandText = "select * from tbl_trans_credit "
                                        + " where credit_uuid = @credit_uuid "
                                        ;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            found = true;
                            var success = Convert.ToInt32(reader["credit_success"].ToString());
                            if (success > 0) done = true;
                        }
                    }

                }

                if (found && !done)
                {
                    var trans = cnn.BeginTransaction();

                    using (var cmd = cnn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@credit_amount", req.credit_amount);

                        cmd.CommandText = "update tbl_player_balance "
                                            + " set player_balance = player_balance + @credit_amount "
                                            + " where merchant_code = @merchant_code "
                                            + " and player_id = @player_id "
                                            ;

                        done = cmd.ExecuteNonQuery() > 0;
                    }

                    if (done)
                    {
                        using (var cmd = cnn.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            dbhelper.AddParam(cmd, "@credit_uuid", req.credit_uuid);
                            dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                            dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                            dbhelper.AddParam(cmd, "@player_id", req.player_id);
                            dbhelper.AddParam(cmd, "@round_id", req.round_id);
                            dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);
                            dbhelper.AddParam(cmd, "@credit_amount", req.credit_amount);
                            dbhelper.AddParam(cmd, "@bet_settle_time", req.bet_settle_time);
                            dbhelper.AddParam(cmd, "@credit_success", done ? 1 : 0);

                            cmd.CommandText = "update tbl_trans_credit "
                                            + " set bet_uuid = @bet_uuid , merchant_code = @merchant_code , player_id = @player_id ,"
                                            + " round_id = @round_id , bet_pool = @bet_pool , credit_amount = @credit_amount , "
                                            + " credit_success = @credit_success , bet_settle_time = @bet_settle_time , update_time = NOW() "
                                            + " where credit_uuid = @credit_uuid "
                                            ;

                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = cnn.CreateCommand())
                        {
                            cmd.Transaction = trans;

                            dbhelper.AddParam(cmd, "@credit_uuid", req.credit_uuid);
                            dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                            dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                            dbhelper.AddParam(cmd, "@player_id", req.player_id);
                            dbhelper.AddParam(cmd, "@round_id", req.round_id);
                            dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);
                            dbhelper.AddParam(cmd, "@credit_amount", req.credit_amount);
                            dbhelper.AddParam(cmd, "@settle_time", req.bet_settle_time);

                            cmd.CommandText = "update tbl_bet_record "
                                            + " set credit_uuid = @credit_uuid , merchant_code = @merchant_code , player_id = @player_id ,"
                                            + " round_id = @round_id , bet_pool = @bet_pool , pay_amount = @credit_amount , "
                                            + " settle_time = @settle_time , update_time = NOW() "
                                            + " where bet_uuid = @bet_uuid "
                                            ;

                            cmd.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                }
                else if (!found && !done)
                {
                    var trans = cnn.BeginTransaction();

                    using (var cmd = cnn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@credit_amount", req.credit_amount);

                        cmd.CommandText = "update tbl_player_balance "
                                            + " set player_balance = player_balance + @credit_amount "
                                            + " where merchant_code = @merchant_code "
                                            + " and player_id = @player_id "
                                            ;

                        done = cmd.ExecuteNonQuery() > 0;
                    }

                    using (var cmd = cnn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        dbhelper.AddParam(cmd, "@credit_uuid", req.credit_uuid);
                        dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@round_id", req.round_id);
                        dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);
                        dbhelper.AddParam(cmd, "@credit_amount", req.credit_amount);
                        dbhelper.AddParam(cmd, "@bet_settle_time", req.bet_settle_time);
                        dbhelper.AddParam(cmd, "@credit_success", done ? 1 : 0);

                        cmd.CommandText = "insert into tbl_trans_credit "
                                        + " set credit_uuid = @credit_uuid , bet_uuid = @bet_uuid , merchant_code = @merchant_code , player_id = @player_id ,"
                                        + " round_id = @round_id , bet_pool = @bet_pool , credit_amount = @credit_amount , "
                                        + " credit_success = @credit_success, bet_settle_time = @bet_settle_time , update_time = NOW() "
                                        ;

                        found = cmd.ExecuteNonQuery() > 0;
                    }

                    using (var cmd = cnn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        dbhelper.AddParam(cmd, "@credit_uuid", req.credit_uuid);
                        dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);
                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);
                        dbhelper.AddParam(cmd, "@round_id", req.round_id);
                        dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);
                        dbhelper.AddParam(cmd, "@credit_amount", done ? req.credit_amount : 0);
                        dbhelper.AddParam(cmd, "@settle_time", req.bet_settle_time);

                        cmd.CommandText = "update tbl_bet_record "
                                        + " set credit_uuid = @credit_uuid , merchant_code = @merchant_code , player_id = @player_id ,"
                                        + " round_id = @round_id , bet_pool = @bet_pool , pay_amount = @credit_amount , "
                                        + " settle_time = @settle_time , update_time = NOW() "
                                        + " where bet_uuid = @bet_uuid "
                                        ;

                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                }

                if (found && done)
                {
                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                        dbhelper.AddParam(cmd, "@player_id", req.player_id);

                        cmd.CommandText = "select * from tbl_player_balance "
                                            + " where merchant_code = @merchant_code "
                                            + " and player_id = @player_id "
                                            ;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                balance = Convert.ToDecimal(reader["player_balance"].ToString());
                            }
                        }
                    }
                }
            }

            ctx.Logger.Info("Credit done");

            var reply = new
            {
                req.merchant_code,
                req.player_id,
                player_balance = balance,
                error_code = found && done ? 0 : -1
            };
            await ctx.Session.Send(ctx.JsonHelper.ToJsonString(reply));
        }

    }
}
