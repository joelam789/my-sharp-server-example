using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.DataAccessServer.BackOfficeData
{
    [Access(Name = "bo-data", IsPublic = false)]
    public class BackOfficeDataService
    {
        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(50);
            node.GetLogger().Info(this.GetType().Name + " service started");
            await Task.Delay(50);

            return "";
        }

        [Access(Name = "check-account")]
        public async Task CheckAccount(RequestContext ctx)
        {
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

            bool okay = false;
            //string sessionId = "";

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase("main"))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@account_id", req.account);
                    dbhelper.AddParam(cmd, "@merchant_code", req.merchant);
                    dbhelper.AddParam(cmd, "@account_pwd", req.password);

                    cmd.CommandText = " select * from tbl_bo_account "
                                            + " where account_id = @account_id and merchant_code = @merchant_code "
                                            + " and account_pwd = @account_pwd and is_active > 0 "
                                            ;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) okay = true;
                    }
                }
            }

            if (okay)
            {
                //sessionId = Guid.NewGuid().ToString();
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = 0,
                    //session_id = sessionId,
                    error_message = "ok"
                }));
            } 
            else
            {
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = 2,
                    error_message = "Invalid account or password"
                }));
            }
        }
        
    }
}
