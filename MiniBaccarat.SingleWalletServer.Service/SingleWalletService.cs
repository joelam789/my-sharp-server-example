using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.SingleWalletServer.Service
{
    [Access(Name = "single-wallet", IsPublic = false)]
    public class SingleWalletService
    {
        [Access(Name = "debit-for-placing-bet")]
        public async Task DebitForBetting(RequestContext ctx)
        {
            ctx.Logger.Info("Debit for placing-bet...");

            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Invalid request"
                }));
                return;
            }

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            ctx.Logger.Info("Create debit record in db...");

            var saveReq = new
            {
                bet_uuid = req.bet_uuid,
                table_code = req.table_code,
                shoe_code = req.shoe_code,
                round_number = req.round_number,
                bet_pool = req.bet_pool,
                merchant_code = req.merchant_code,
                player_id = req.player_id,
                bet_amount = req.bet_amount
            };
            string dbReplyStr = await RemoteCaller.RandomCall(ctx.RemoteServices,
                "transaction-data", "create-debit", ctx.JsonCodec.ToJsonString(saveReq));

            if (dbReplyStr.Trim().Length <= 0 || !dbReplyStr.Contains("{"))
            {
                await ctx.Session.Send("Failed to create debit record in db: " + dbReplyStr);
                return;
            }

            ctx.Logger.Info("Call merchant site to debit...");

            dynamic dbReply = ctx.JsonCodec.ToJsonObject(dbReplyStr);
            string apiUrl = dbReply.request_url;

            var apiReq = new
            {
                dbReply.debit_uuid,
                req.bet_uuid,
                req.merchant_code,
                req.player_id,
                dbReply.round_id,
                req.bet_pool,
                debit_amount = req.bet_amount,
                req.bet_time,
                is_cancelled = false
            };
            dynamic ret = await RemoteCaller.Request(apiUrl, apiReq, 10 * 1000);

            if (ret == null)
            {
                ctx.Logger.Info("Failed to call debit function from merchant site");

                var updateReq = new
                {
                    dbReply.debit_uuid,
                    request_times = 1,
                    is_success = 0,
                    network_error = 1,
                    response_error = 0
                };
                string dbReplyStr2 = await RemoteCaller.RandomCall(ctx.RemoteServices,
                    "transaction-data", "update-debit", ctx.JsonCodec.ToJsonString(updateReq));

                dynamic dbReply2 = ctx.JsonCodec.ToJsonObject(dbReplyStr2);

                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Failed to call debit function from merchant site"
                }));
            }
            else
            {
                ctx.Logger.Info("Update debit record in db...");

                if (ret.error_code == 0)
                {
                    var updateReq = new
                    {
                        dbReply.debit_uuid,
                        req.merchant_code,
                        req.player_id,
                        request_times = 1,
                        is_success = 1,
                        network_error = 0,
                        response_error = 0
                    };
                    string dbReplyStr2 = await RemoteCaller.RandomCall(ctx.RemoteServices,
                        "transaction-data", "update-debit", ctx.JsonCodec.ToJsonString(updateReq));

                    if (String.IsNullOrEmpty(dbReplyStr2))
                    {
                        ctx.Logger.Info("Failed to update debit record in db");
                    }
                    else
                    {
                        dynamic dbReply2 = ctx.JsonCodec.ToJsonObject(dbReplyStr2);
                        ctx.Logger.Info("Update debit record in db - error code: " + dbReply2.error_code.ToString());
                    }
                }
                else
                {
                    var updateReq = new
                    {
                        dbReply.debit_uuid,
                        request_times = 1,
                        is_success = 0,
                        network_error = 0,
                        response_error = ret.error_code
                    };
                    string dbReplyStr2 = await RemoteCaller.RandomCall(ctx.RemoteServices,
                        "transaction-data", "update-debit", ctx.JsonCodec.ToJsonString(updateReq));

                    dynamic dbReply2 = ctx.JsonCodec.ToJsonObject(dbReplyStr2);

                }

                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(ret));
            }
        }

        [Access(Name = "credit-for-settling-bet")]
        public async Task CreditForBetting(RequestContext ctx)
        {
        }
    }
}
