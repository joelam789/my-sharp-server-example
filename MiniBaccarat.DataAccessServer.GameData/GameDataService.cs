using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.DataAccessServer.GameData
{
    [Access(Name = "game-data", IsPublic = false)]
    public class GameDataService
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

        [Access(Name = "save-record")]
        public async Task SaveGameRecord(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            bool okay = false;

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase("main"))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@server_code", req.server);
                    dbhelper.AddParam(cmd, "@table_code", req.table);
                    dbhelper.AddParam(cmd, "@shoe_code", req.shoe);
                    dbhelper.AddParam(cmd, "@round_number", req.round);
                    dbhelper.AddParam(cmd, "@round_state", req.state);
                    dbhelper.AddParam(cmd, "@player_cards", req.player);
                    dbhelper.AddParam(cmd, "@banker_cards", req.banker);
                    dbhelper.AddParam(cmd, "@game_result", req.result);
                    dbhelper.AddParam(cmd, "@round_start_time", req.starttime);
                    dbhelper.AddParam(cmd, "@last_update_time", req.updatetime);

                    cmd.CommandText = " insert into tbl_game_record "
                                            + " ( server_code, table_code, shoe_code, round_number, round_state,  "
                                            + "   player_cards, banker_cards, game_result, round_start_time, last_update_time ) values "
                                            + " ( @server_code , @table_code , @shoe_code , @round_number , @round_state , "
                                            + "   @player_cards , @banker_cards , @game_result, @round_start_time, @last_update_time ) "
                                            ;

                    okay = cmd.ExecuteNonQuery() > 0;
                }
            }

            if (okay) await ctx.Session.Send("ok");
            else await ctx.Session.Send("Failed to update database");
        }

        [Access(Name = "update-result")]
        public async Task UpdateGameResult(RequestContext ctx)
        {
            string reqstr = ctx.Data.ToString();
            if (reqstr.Trim().Length <= 0)
            {
                await ctx.Session.Send("Invalid request");
                return;
            }

            bool okay = false;

            dynamic req = ctx.JsonCodec.ToJsonObject(reqstr);

            var dbhelper = ctx.DataHelper;
            using (var cnn = dbhelper.OpenDatabase("main"))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@server_code", req.server);
                    dbhelper.AddParam(cmd, "@table_code", req.table);
                    dbhelper.AddParam(cmd, "@shoe_code", req.shoe);
                    dbhelper.AddParam(cmd, "@round_number", req.round);
                    dbhelper.AddParam(cmd, "@round_state", req.state);
                    dbhelper.AddParam(cmd, "@player_cards", req.player);
                    dbhelper.AddParam(cmd, "@banker_cards", req.banker);
                    dbhelper.AddParam(cmd, "@game_result", req.result);
                    dbhelper.AddParam(cmd, "@last_update_time", req.updatetime);

                    cmd.CommandText = "update tbl_game_record "
                                            + " set round_state = @round_state "
                                            + " , player_cards = @player_cards "
                                            + " , banker_cards = @banker_cards "
                                            + " , game_result = @game_result "
                                            + ", last_update_time = @last_update_time "
                                            + " where server_code = @server_code and table_code = @table_code " 
                                            + " and shoe_code = @shoe_code and round_number = @round_number "
                                            ;
                    okay = cmd.ExecuteNonQuery() > 0;
                }
            }

            if (okay) await ctx.Session.Send("ok");
            else await ctx.Session.Send("Failed to update database");
        }
    }
}
