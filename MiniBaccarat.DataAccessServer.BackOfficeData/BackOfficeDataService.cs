using System;
using System.Collections.Generic;
using System.Dynamic;
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
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = -1,
                    error_message = "Invalid request"
                }));
                return;
            }

            bool okay = false;
            //string sessionId = "";

            dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);

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
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = 0,
                    //session_id = sessionId,
                    error_message = "ok"
                }));
            } 
            else
            {
                await ctx.Session.Send(ctx.JsonHelper.ToJsonString(new
                {
                    error_code = 2,
                    error_message = "Invalid account or password"
                }));
            }
        }

        [Access(Name = "get-game-results")]
        public async Task GetGameResults(RequestContext ctx)
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

            bool okay = false;

            dynamic reply = new ExpandoObject();
            reply.rows = new List<ExpandoObject>();

            dynamic req = ctx.JsonHelper.ToJsonObject(reqstr);

            string pageSizeStr = req.pageSize;
            string pageNumberStr = req.pageNumber;

            int pageSize = Convert.ToInt32(pageSizeStr);
            int pageNumber = Convert.ToInt32(pageNumberStr);

            if (pageSize <= 0) pageSize = 1;
            if (pageNumber <= 0) pageNumber = 1;

            string sqlwhere = " where round_state >= 9 "
                            + " and round_start_time >= @first_game_time and round_start_time <= @last_game_time "
                            ;

            string sqlorder = " order by round_start_time ";

            int total = 0;
            int pageCount = 0;

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase("main"))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@first_game_time", req.fromGameTime);
                    dbhelper.AddParam(cmd, "@last_game_time", req.toGameTime);

                    cmd.CommandText = " select count(game_id) from tbl_game_record " + sqlwhere;
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }

                reply.total = total >= 0 ? total : 0;

                if (total > 0)
                {
                    int rest = total % pageSize;
                    pageCount = (total - rest) / pageSize;
                    if (rest > 0) pageCount += 1;

                    int offset = pageSize * (pageNumber - 1);
                    if (offset >= total)
                    {
                        pageNumber = pageCount;
                        offset = pageSize * (pageNumber - 1);
                    }

                    if (offset + pageSize > total) pageSize = total - offset;

                    using (var cmd = cnn.CreateCommand())
                    {
                        dbhelper.AddParam(cmd, "@first_game_time", req.fromGameTime);
                        dbhelper.AddParam(cmd, "@last_game_time", req.toGameTime);

                        cmd.CommandText = " select * from tbl_game_record ";
                        cmd.CommandText += sqlwhere + sqlorder;
                        cmd.CommandText += " limit " + offset + "," + pageSize;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dynamic row = new ExpandoObject();
                                
                                row.table_code = reader["table_code"];
                                row.shoe_code = reader["shoe_code"];
                                row.round_number = reader["round_number"];
                                row.game_time = reader["round_start_time"];
                                row.game_result = reader["banker_cards"].ToString() + " : " + reader["player_cards"].ToString();

                                reply.rows.Add(row);
                            }
                        }
                    }
                }
                okay = true;
            }

            if (okay)
            {
                reply.error_code = 0;
                reply.error_message = "ok";
            }
            else
            {
                reply.error_code = 1;
                reply.error_message = "Failed to get game results from DB";
            }

            await ctx.Session.Send(ctx.JsonHelper.ToJsonString(reply));
        }

    }
}
