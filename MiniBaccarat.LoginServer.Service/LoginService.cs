using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.LoginServer.Service
{
    [Access(Name = "login")]
    public class LoginService
    {
        protected string m_MainCache = "MainCache";
        protected IServerNode m_LocalNode = null;

        [Access(Name = "on-load", IsLocal = true)]
        public async Task<string> Load(IServerNode node)
        {
            //System.Diagnostics.Debugger.Break();

            await Task.Delay(50);
            m_LocalNode = node;
            await Task.Delay(50);

            node.GetLogger().Info(this.GetType().Name + " service started");

            return "";
        }

        public string RandomPickPublicServiceUrl(Dictionary<string, List<string>> remoteServices, string serviceName)
        {
            var publicUrl = "";
            List<string> serviceList = null;
            if (remoteServices != null && remoteServices.TryGetValue(serviceName, out serviceList))
            {
                if (serviceList != null && serviceList.Count > 0)
                {
                    var remoteInfoParts = RandomPicker.Pick<string>(serviceList).Split('|');
                    if (remoteInfoParts.Length >= 2) // name | url | key
                    {
                        var urls = remoteInfoParts[1].Split(',');
                        if (urls.Length >= 2) publicUrl = urls[1];
                    }
                }
            }
            return publicUrl;
        }

        [Access(Name = "player-login")]
        public async Task PlayerLogin(RequestContext ctx)
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

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            string merchantUrl = await RemoteCaller.RandomCall(m_LocalNode.GetRemoteServices(),
                                        "merchant-data", "get-merchant-url", req.merchant_code.ToString());

            if (String.IsNullOrEmpty(merchantUrl))
            {
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Merchant API URL not found: " + req.merchant_code.ToString()
                }));
                return;
            }

            ctx.Logger.Info("Player login - [" + req.merchant_code.ToString() + "] " + req.player_id.ToString());
            ctx.Logger.Info("Merchant URL - " + merchantUrl);

            var apiReq = new
            {
                req.merchant_code,
                req.player_id,
                req.login_token
            };
            dynamic ret = await RemoteCaller.Request(merchantUrl + "/player/validate-login", apiReq, null, 10 * 1000);

            if (ret == null || ret.error_code != 0)
            {
                ctx.Logger.Error("Three-Way Login Error: " + (ret == null ? "Failed to call merchant API" : ret.error_message));
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Three-Way Login Error"
                }));
                return;
            }

            var okay = false;
            var sessionId = Guid.NewGuid().ToString();

            var dbhelper = m_LocalNode.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@session_id", sessionId);
                    dbhelper.AddParam(cmd, "@merchant_code", req.merchant_code);
                    dbhelper.AddParam(cmd, "@player_id", req.player_id);

                    cmd.CommandText = " update tbl_player_session "
                                           + " set session_id = @session_id , update_time = NOW() "
                                           + " where merchant_code = @merchant_code and player_id = @player_id ";

                    okay = cmd.ExecuteNonQuery() > 0;

                    if (!okay)
                    {
                        using (var cmd2 = cnn.CreateCommand())
                        {
                            dbhelper.AddParam(cmd2, "@session_id", sessionId);
                            dbhelper.AddParam(cmd2, "@merchant_code", req.merchant_code);
                            dbhelper.AddParam(cmd2, "@player_id", req.player_id);

                            cmd2.CommandText = " insert into tbl_player_session "
                                                   + " ( session_id , merchant_code, player_id, update_time ) values "
                                                   + " ( @session_id, @merchant_code, @player_id, NOW() ) ";

                            okay = cmd2.ExecuteNonQuery() > 0;
                        }
                    }
                }
            }

            if (!okay)
            {
                ctx.Logger.Error("Three-Way Login Failed!");
                await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Three-Way Login Failed"
                }));
                return;
            }

            var remoteServices = m_LocalNode.GetRemoteServices();

            var frontEndUrl = RandomPickPublicServiceUrl(remoteServices, "table-info");
            var betServerUrl = RandomPickPublicServiceUrl(remoteServices, "accept-bet");

            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
            {
                req.merchant_code,
                req.player_id,
                ret.player_balance,
                session_id = sessionId,
                front_end = frontEndUrl,
                bet_server = betServerUrl,
                error_code = 0,
                error_message = "Three-Way Login Okay"
            }));

        }
    }
}
