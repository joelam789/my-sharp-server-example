using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.DataAccessServer.TransData
{
    [Access(Name = "transaction-data", IsPublic = false)]
    public class TransactionDataService
    {
        [Access(Name = "create-debit")]
        public async Task CreateDebit(RequestContext ctx)
        {
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

            bool okay = false;
            string debitId = req.bet_uuid + "-debit";
            string gameCode = "Mini-" + req.table_code;
            string roundId = req.table_code + "-" + req.shoe_code + "-" + req.round_number;

            string providerCode = "mini";

            string merchantCode = req.merchant_code.ToString();

            string merchantUrl = await RemoteCaller.RandomCall(ctx.RemoteServices, "merchant-data", "get-merchant-url", merchantCode);

            if (String.IsNullOrEmpty(merchantUrl))
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Merchant API URL not found: " + req.merchant_code.ToString()
                }));
                return;
            }

            string requestUrl = merchantUrl + "/bet/debit-for-placing-bet";

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@debit_uuid", debitId);
                    dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);

                    dbhelper.AddParam(cmd, "@game_code", gameCode);
                    dbhelper.AddParam(cmd, "@round_id", roundId);
                    dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);

                    dbhelper.AddParam(cmd, "@provider_code", providerCode);

                    dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                    dbhelper.AddParam(cmd, "@player_id", req.player_id);
                    
                    dbhelper.AddParam(cmd, "@debit_amount", req.bet_amount);

                    dbhelper.AddParam(cmd, "@request_url", requestUrl);

                    cmd.CommandText = " insert into tbl_trans_debit "
                                    + " ( debit_uuid, bet_uuid, game_code, round_id, bet_pool, provider_code, merchant_code, player_id, debit_amount, request_url ) values "
                                    + " ( @debit_uuid, @bet_uuid, @game_code, @round_id , @bet_pool , @provider_code , @merchant_code , @player_id , @debit_amount , @request_url ) "
                                    ;

                    okay = cmd.ExecuteNonQuery() > 0;
                }
            }

            if (okay)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = 0,
                    debit_uuid = debitId,
                    round_id = roundId,
                    request_url = requestUrl
                }));
            }
            else await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
            {
                error_code = -1,
                error_message = "Failed to update database"
            }));
        }

        [Access(Name = "update-debit")]
        public async Task UpdateDebit(RequestContext ctx)
        {
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

            bool okay = false;
            string debitId = req.debit_uuid;
            string merchantCode = req.merchant_code.ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@debit_uuid", debitId);

                    dbhelper.AddParam(cmd, "@request_times", req.request_times);

                    dbhelper.AddParam(cmd, "@is_success", req.is_success);
                    dbhelper.AddParam(cmd, "@network_error", req.network_error);
                    dbhelper.AddParam(cmd, "@response_error", req.response_error);


                    cmd.CommandText = " update tbl_trans_debit "
                                    + " set request_times = @request_times, "
                                    + " is_success = @is_success, "
                                    + " network_error = @network_error, "
                                    + " response_error = @response_error, "
                                    + " update_time = NOW() "
                                    + " where debit_uuid = @debit_uuid "
                                    ;

                    okay = cmd.ExecuteNonQuery() > 0;
                }
            }

            if (okay)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = 0
                }));
            }
            else await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
            {
                error_code = -1,
                error_message = "Failed to update database"
            }));
        }

        [Access(Name = "create-credit")]
        public async Task CreateCredit(RequestContext ctx)
        {
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

            bool okay = false;
            string creditId = req.bet_uuid + "-credit";
            string gameCode = "Mini-" + req.table_code;
            string roundId = req.table_code + "-" + req.shoe_code + "-" + req.round_number;

            string providerCode = "mini";

            string merchantCode = req.merchant_code.ToString();

            string merchantUrl = await RemoteCaller.RandomCall(ctx.RemoteServices, "merchant-data", "get-merchant-url", merchantCode);

            if (String.IsNullOrEmpty(merchantUrl))
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Merchant API URL not found: " + req.merchant_code.ToString()
                }));
                return;
            }

            string requestUrl = merchantUrl + "/bet/credit-for-settling-bet";

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@credit_uuid", creditId);
                    dbhelper.AddParam(cmd, "@bet_uuid", req.bet_uuid);

                    dbhelper.AddParam(cmd, "@game_code", gameCode);
                    dbhelper.AddParam(cmd, "@round_id", roundId);
                    dbhelper.AddParam(cmd, "@bet_pool", req.bet_pool);

                    dbhelper.AddParam(cmd, "@provider_code", providerCode);

                    dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                    dbhelper.AddParam(cmd, "@player_id", req.player_id);

                    dbhelper.AddParam(cmd, "@credit_amount", req.pay_amount);

                    dbhelper.AddParam(cmd, "@request_url", requestUrl);

                    cmd.CommandText = " insert into tbl_trans_credit "
                                    + " ( credit_uuid, bet_uuid, game_code, round_id, bet_pool, provider_code, merchant_code, player_id, credit_amount, request_url ) values "
                                    + " ( @credit_uuid, @bet_uuid, @game_code, @round_id , @bet_pool , @provider_code , @merchant_code , @player_id , @credit_amount , @request_url ) "
                                    ;

                    okay = cmd.ExecuteNonQuery() > 0;
                }
            }

            if (okay)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = 0,
                    credit_uuid = creditId,
                    round_id = roundId,
                    request_url = requestUrl
                }));
            }
            else await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
            {
                error_code = -1,
                error_message = "Failed to update database"
            }));
        }

        [Access(Name = "update-credit")]
        public async Task UpdateCredit(RequestContext ctx)
        {
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

            bool okay = false;
            string creditId = req.credit_uuid;
            string merchantCode = req.merchant_code.ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(merchantCode))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@credit_uuid", creditId);

                    dbhelper.AddParam(cmd, "@request_times", req.request_times);

                    dbhelper.AddParam(cmd, "@is_success", req.is_success);
                    dbhelper.AddParam(cmd, "@network_error", req.network_error);
                    dbhelper.AddParam(cmd, "@response_error", req.response_error);


                    cmd.CommandText = " update tbl_trans_credit "
                                    + " set request_times = @request_times, "
                                    + " is_success = @is_success, "
                                    + " network_error = @network_error, "
                                    + " response_error = @response_error, "
                                    + " update_time = NOW() "
                                    + " where credit_uuid = @credit_uuid "
                                    ;

                    okay = cmd.ExecuteNonQuery() > 0;
                }
            }

            if (okay)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = 0
                }));
            }
            else await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
            {
                error_code = -1,
                error_message = "Failed to update database"
            }));
        }
    }
}
