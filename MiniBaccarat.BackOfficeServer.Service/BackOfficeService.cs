using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.BackOfficeServer.Service
{
    [Access(Name = "bo-service")]
    public class BackOfficeService
    {
        protected string m_MainCache = "MainCache";

        [Access(Name = "validate-request")]
        public string ValidateRequest(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                return "Invalid request";
            }

            //dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);
            var req = ctx.JsonHelper.ToDictionary(reqstr);
            string sessionId = req.ContainsKey("session_id")? req["session_id"].ToString() 
                                : (req.ContainsKey("sessionId") ? req["sessionId"].ToString() : "");

            var okay = false;

            if (!String.IsNullOrEmpty(sessionId))
            {
                var dbhelper = ctx.DataHelper;
                using (var cnn = dbhelper.OpenDatabase(m_MainCache))
                {
                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@session_id", sessionId);

                        cmd.CommandText = " UPDATE tbl_bo_session "
                                               + " SET last_access_time = NOW() "
                                               + " WHERE session_id = @session_id "
                                               + " AND TIMESTAMPDIFF(SECOND, last_access_time, NOW()) <= 180 " // session timeout in 3 mins
                                               ;

                        okay = cmd.ExecuteNonQuery() > 0;
                    }
                }
            }

            if (!okay)
            {
                ctx.Logger.Info("Invalid or expired backoffice session: " + sessionId);
                return "Invalid or expired backoffice session";
            }
            else ctx.Logger.Info("Backoffice session is ok: " + sessionId);

            return "";
        }

        [Access(Name = "check-session")]
        public async Task CheckSession(RequestContext ctx)
        {
            await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
            {
                error = 0
            }));
        }

        [Access(Name = "get-game-results")]
        public async Task GetGameResults(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                return;
            }

            //dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);
            var req = ctx.JsonHelper.ToDictionary(reqstr);

            string sessionId = req.ContainsKey("sessionId") ? req["sessionId"].ToString() : "";

            IDictionary<string, object> queryParam = req.ContainsKey("queryParam") ? req["queryParam"] as IDictionary<string, object> : null;

            if (queryParam == null)
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    total = 0,
                    rows = new List<dynamic>(),
                    error_code = 1,
                    error_message = "Failed to get game results from DB",
                }));
                return;
            }

            string pageSize = queryParam.ContainsKey("rows") ? queryParam["rows"].ToString() : "1";
            string pageNumber = queryParam.ContainsKey("page") ? queryParam["page"].ToString() : "1";

            string merchantCode = queryParam.ContainsKey("merchantCode") ? queryParam["merchantCode"].ToString() : "";
            string userId = queryParam.ContainsKey("userId") ? queryParam["userId"].ToString() : "";
            string fromDateTime = queryParam.ContainsKey("fromDateTime") ? queryParam["fromDateTime"].ToString() : "";
            string toDateTime = queryParam.ContainsKey("toDateTime") ? queryParam["toDateTime"].ToString() : "";

            //ctx.Logger.Info("SessionID is: " + sessionId);
            //ctx.Logger.Info("Page Size is: " + pageSize);
            //ctx.Logger.Info("Page is: " + pageNumber);
            //ctx.Logger.Info("merchantCode is: " + merchantCode);
            //ctx.Logger.Info("fromDateTime is: " + fromDateTime);

            var dbReq = new
            {
                pageSize,
                pageNumber,
                fromGameTime = fromDateTime,
                toGameTime = toDateTime
            };
            string replystr = await RemoteCaller.RandomCall(ctx.RemoteServices,
                "bo-data", "get-game-results", ctx.JsonHelper.ToJsonString(dbReq));

            if (String.IsNullOrEmpty(replystr))
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    total = 0,
                    rows = new List<dynamic>(),
                    error_code = 1,
                    error_message = "Failed to get game results from DB",
                }));
            }
            else
            {
                await ctx.Session.Send(replystr);
            }
        }

    }
}
