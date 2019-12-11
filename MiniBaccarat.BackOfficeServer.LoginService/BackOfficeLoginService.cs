using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BackOfficeServer.LoginService
{
    [Access(Name = "bo-login-service")]
    public class BackOfficeLoginService
    {
        protected string m_MainCache = "MainCache";

        [Access(Name = "user-login")]
        public async Task UserLogin(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

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

            var dataReq = new
            {
                req.account,
                req.merchant,
                req.password
            };

            string dataReplyString = await RemoteCaller.RandomCall(ctx.RemoteServices,
                                        "bo-data", "check-account", ctx.JsonCodec.ToJsonString(dataReq));

            if (String.IsNullOrEmpty(dataReplyString))
            {
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Failed to check backoffice account from db: [" + req.merchant.ToString() + "]" + req.account.ToString()
                }));
                return;
            }

            dynamic dataReply = ctx.JsonCodec.ToJsonObject(dataReplyString);
            if (dataReply.error_code != 0)
            {
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    dataReply.error_code,
                    error_message = "Failed to validate backoffice account from db: [" + req.merchant.ToString() + "]"
                                    + req.account.ToString() + " - " + dataReply.error_message.ToString()
                }));
                return;
            }

            ctx.Logger.Info("Backoffice user login - [" + req.merchant.ToString() + "] " + req.account.ToString());

            var okay = false;
            var sessionId = Guid.NewGuid().ToString();

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@session_id", sessionId);
                    dbhelper.AddParam(cmd, "@account_id", req.account);
                    dbhelper.AddParam(cmd, "@merchant_code", req.merchant);

                    cmd.CommandText = " update tbl_bo_session "
                                           + " set session_id = @session_id , last_access_time = NOW() "
                                           + " where merchant_code = @merchant_code and account_id = @account_id ";

                    okay = cmd.ExecuteNonQuery() > 0;

                    if (!okay)
                    {
                        using (var cmd2 = cnn.CreateCommand())
                        {
                            dbhelper.AddParam(cmd2, "@session_id", sessionId);
                            dbhelper.AddParam(cmd2, "@account_id", req.account);
                            dbhelper.AddParam(cmd2, "@merchant_code", req.merchant);

                            cmd2.CommandText = " insert into tbl_bo_session "
                                                + " ( session_id , account_id , merchant_code, last_access_time ) values "
                                                + " ( @session_id, @account_id, @merchant_code, NOW() ) ";

                            okay = cmd2.ExecuteNonQuery() > 0;
                        }
                    }
                }
            }

            if (!okay)
            {
                ctx.Logger.Error("Failed to let backoffice user login: cache error");
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Failed to let backoffice user login: cache error"
                }));
                return;
            }

            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
            {
                session_id = sessionId,
                req.account,
                req.merchant,
                error_code = 0,
                error_message = "ok"
            }));

        }
    }
}
