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
        protected string m_MainCache = "SharpNode";

        [Access(Name = "validate-request")]
        public string ValidateRequest(RequestContext ctx)
        {
            //System.Diagnostics.Debugger.Break();

            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                return "Invalid request";
            }

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);
            string sessionId = req.session_id.ToString();

            var okay = false;

            if (!String.IsNullOrEmpty(sessionId))
            {
                var dbhelper = ctx.DataHelper;
                using (var cnn = dbhelper.OpenDatabase(m_MainCache))
                {
                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@session_id", sessionId);

                        cmd.CommandText = " UPDATE db_mini_baccarat.tbl_bo_session "
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
            await ctx.Session.Send(ctx.JsonCodec.ToJsonString(new
            {
                error = 0
            }));
        }



    }
}
