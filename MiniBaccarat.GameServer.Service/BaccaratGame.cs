using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.GameServer.Service
{
    public class BaccaratGame
    {
        static List<string> CARD_SUITS = new List<string>
        {
            "S" /* Spade (♠) */,
            "H" /* Heart (♥) */,
            "C" /* Club (♣) */,
            "D" /* Diamond (♦) */,

        };
        static List<string> CARD_VALUES = new List<string>
        {
            "A" /* Ace */,
            "2" /* 2 */,
            "3" /* 3 */,
            "4" /* 4 */,
            "5" /* 5 */,
            "6" /* 6 */,
            "7" /* 7 */,
            "8" /* 8 */,
            "9" /* 9 */,
            "T" /* 10 */,
            "J" /* Jack */,
            "Q" /* Queen */,
            "K" /* King */,

        };

        public enum GAME_STATUS 
        {
            Unknown = 0,
            NotWorking,
            GetGameReady,
            StartNewRound,
            BettingTime,
            DealingFirst4Cards,
            DealingLastPlayerCard,
            DealingLastBankerCard,
            CountingPoints,
            OutputGameResult
        };

        private List<string> m_GameStatus = new List<string>
        {
            "Unknown",
            "NotWorking",
            "GetGameReady",
            "StartNewRound",
            "BettingTime",
            "DealingFirst4Cards",
            "DealingLastPlayerCard",
            "DealingLastBankerCard",
            "CountingPoints",
            "OutputGameResult"
        };

        static CommonRng m_Rng = new CommonRng();

        static string GenCard()
        {
            return CARD_VALUES[m_Rng.Next(CARD_VALUES.Count)] + CARD_SUITS[m_Rng.Next(CARD_SUITS.Count)];
        }

        public static int GetCardValue(string card)
        {
            int idx = CARD_VALUES.IndexOf(card.Trim().ToUpper());
            if (idx < 0) throw new Exception("Wrong card value: " + card);
            int value = idx + 1;
            if (value >= 10) value = 0;
            return value;
        }

        private Timer m_Timer = null;

        private IServerNode m_Node = null;
        private IServerLogger m_Logger = null;

        private GAME_STATUS m_GameState = GAME_STATUS.Unknown;

        private int m_GameReadyCountdown = -1;
        private int m_BettingTimeCountdown = -1;

        private bool m_WaitingForDealing = false;

        private bool m_IsRunningGameLoop = false;

        private string m_GameCode = "";
        private int m_RoundIndex = 0;

        private DateTime m_RoundStartTime = DateTime.MinValue;
        private DateTime m_RoundUpdateTime = DateTime.MinValue;

        private Queue<int> m_History = new Queue<int>();

        private string m_MainCache = "SharpNode";

        public static readonly int GET_READY_COUNTDOWN = 3;
        public static readonly int BET_TIME_COUNTDOWN = 8;
        public static readonly int MAX_HIST_LENGTH = 10;

        public int PlayerPoints { get; private set; } = -1;
        public int BankerPoints { get; private set; } = -1;

        public BaccaratGame(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();

            m_GameState = GAME_STATUS.Unknown;
            m_WaitingForDealing = false;
            m_IsRunningGameLoop = false;

            PlayerPoints = -1;
            BankerPoints = -1;

            m_GameCode = "";
            m_RoundIndex = 0;
        }

        private List<string> m_PlayerCards = new List<string>();
        private List<string> m_BankerCards = new List<string>();

        public string GetGameId()
        {
            return m_Node.GetName() + "-" + m_GameCode + "-" + m_RoundIndex;
        }

        private dynamic Snapshot()
        {
            int gameResult = 0;
            int playerPoints = GetPlayerPoints();
            int bankerPoints = GetBankerPoints();
            if (playerPoints < bankerPoints) gameResult = 1;
            if (playerPoints > bankerPoints) gameResult = 2;  
            if (playerPoints == bankerPoints) gameResult = 3;
            gameResult = gameResult * 100 + bankerPoints * 10 + playerPoints;

            var currentGameState = new
            {
                server = m_Node.GetName(),
                game = m_GameCode,
                round = m_RoundIndex,
                state = (int)m_GameState,
                status = m_GameStatus[(int)m_GameState],
                countdown = m_BettingTimeCountdown,
                starttime = m_RoundStartTime,
                updatetime = m_RoundUpdateTime,
                history = String.Join(",", m_History.ToArray()),
                player = String.Join(",", m_PlayerCards.ToArray()),
                banker = String.Join(",", m_BankerCards.ToArray()),
                //result = GetPlayerPoints() + "," + GetBankerPoints()
                result = gameResult
            };


            //return m_Node.GetJsonHelper().ToJsonString(currentGameState);
            return currentGameState;
        }

        public void Start()
        {
            Stop();
            m_RoundIndex = 0;
            //m_GameCode = Guid.NewGuid().ToString();
            m_GameCode = DateTime.Now.ToString("yyyyMMddHHmmss");
            m_History.Clear();
            m_GameReadyCountdown = GET_READY_COUNTDOWN;
            m_GameState = GAME_STATUS.GetGameReady;
            m_WaitingForDealing = false;
            m_IsRunningGameLoop = false;
            m_Timer = new Timer(Tick, m_Rng, 500, 1000 * 1);
        }

        public void Stop()
        {
            m_GameState = GAME_STATUS.NotWorking;
            if (m_Timer != null)
            {
                Thread.Sleep(500);
                m_Timer.Dispose();
                m_Timer = null;
            }
            m_PlayerCards.Clear();
            m_BankerCards.Clear();
            m_GameReadyCountdown = -1;
            m_BettingTimeCountdown = -1;
            PlayerPoints = -1;
            BankerPoints = -1;
            m_IsRunningGameLoop = false;
        }

        private void UpdateRoundState(GAME_STATUS nextState)
        {
            if (m_GameState != GAME_STATUS.Unknown && m_GameState != GAME_STATUS.NotWorking)
            {
                m_RoundUpdateTime = DateTime.Now;
                dynamic currentRoundState = Snapshot();

                if (currentRoundState != null)
                {
                    try
                    {
                        CacheSnapshot(currentRoundState);
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Error("Faild to cache snapshot: " + ex.ToString());
                        m_Logger.Error(ex.StackTrace);
                    }

                }

                m_GameState = nextState;

                m_Logger.Info("CurrentRoundState - [" + currentRoundState.status + "]");
            }
        }

        private void CacheSnapshot(dynamic snapshot)
        {
            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainCache))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    dbhelper.AddParam(cmd, "@server_code", snapshot.server);
                    dbhelper.AddParam(cmd, "@game_code", snapshot.game);
                    dbhelper.AddParam(cmd, "@round_number", snapshot.round);
                    dbhelper.AddParam(cmd, "@round_state", snapshot.state);
                    dbhelper.AddParam(cmd, "@round_state_text", snapshot.status);
                    dbhelper.AddParam(cmd, "@bet_time_countdown", snapshot.countdown);
                    dbhelper.AddParam(cmd, "@player_cards", snapshot.player);
                    dbhelper.AddParam(cmd, "@banker_cards", snapshot.banker);
                    dbhelper.AddParam(cmd, "@game_result", snapshot.result);
                    dbhelper.AddParam(cmd, "@game_history", snapshot.history);
                    dbhelper.AddParam(cmd, "@round_start_time", snapshot.starttime);
                    dbhelper.AddParam(cmd, "@round_update_time", snapshot.updatetime);

                    switch (m_GameState)
                    {
                        case (GAME_STATUS.Unknown): break;
                        case (GAME_STATUS.NotWorking): break;
                        case (GAME_STATUS.GetGameReady):
                            break;
                        case (GAME_STATUS.StartNewRound):
                            cmd.CommandText = "update db_mini_baccarat.tbl_round_state "
                                            + " set backup_number = backup_number + 1 "
                                            + " where server_code = @server_code ; ";
                            cmd.CommandText = cmd.CommandText + "delete from db_mini_baccarat.tbl_round_state "
                                            + " where server_code = @server_code and backup_number > 3 ; ";
                            cmd.CommandText = cmd.CommandText + " insert into db_mini_baccarat.tbl_round_state "
                                            + " ( server_code, game_code, round_number, round_state, round_state_text, bet_time_countdown, "
                                            + "   player_cards, banker_cards, game_result, game_history, round_start_time, round_update_time ) values "
                                            + " ( @server_code , @game_code , @round_number , @round_state , @round_state_text , @bet_time_countdown , "
                                            + "   @player_cards , @banker_cards , @game_result, @game_history, @round_start_time, @round_update_time ) "
                                            ;

                            cmd.ExecuteNonQuery();
                            break;
                        case (GAME_STATUS.BettingTime):
                            cmd.CommandText = "update db_mini_baccarat.tbl_round_state "
                                            + " set round_state = @round_state "
                                            + " , round_state_text = @round_state_text "
                                            + " , bet_time_countdown = @bet_time_countdown "
                                            + ", round_update_time = @round_update_time "
                                            + " where server_code = @server_code and game_code = @game_code and round_number = @round_number "
                                            ;
                            cmd.ExecuteNonQuery();
                            break;
                        case (GAME_STATUS.DealingFirst4Cards):
                        case (GAME_STATUS.DealingLastPlayerCard):
                        case (GAME_STATUS.DealingLastBankerCard):
                        case (GAME_STATUS.CountingPoints):
                        case (GAME_STATUS.OutputGameResult):
                            cmd.CommandText = "update db_mini_baccarat.tbl_round_state "
                                            + " set round_state = @round_state "
                                            + " , round_state_text = @round_state_text "
                                            + " , player_cards = @player_cards "
                                            + " , banker_cards = @banker_cards "
                                            + " , game_result = @game_result "
                                            + " , game_history = @game_history "
                                            + ", round_update_time = @round_update_time "
                                            + " where server_code = @server_code and game_code = @game_code and round_number = @round_number "
                                            ;
                            cmd.ExecuteNonQuery();
                            break;
                    }
                }
                    
            }
        }

        public async Task GameLoop()
        {
            switch (m_GameState)
            {
                case (GAME_STATUS.Unknown): break;
                case (GAME_STATUS.NotWorking): break;
                case (GAME_STATUS.GetGameReady):
                    GetGameReady();
                    break;
                case (GAME_STATUS.StartNewRound):
                    StartNewRound();
                    break;
                case (GAME_STATUS.BettingTime):
                    StartBettingTime();
                    break;
                case (GAME_STATUS.DealingFirst4Cards):
                    DealingFirst4Cards();
                    break;
                case (GAME_STATUS.DealingLastPlayerCard):
                    DealingLastPlayerCard();
                    break;
                case (GAME_STATUS.DealingLastBankerCard):
                    DealingLastBankerCard();
                    break;
                case (GAME_STATUS.CountingPoints):
                    CountingPoints();
                    break;
                case (GAME_STATUS.OutputGameResult):
                    await OutputGameResult();
                    break;
            }
            
        }

        private async void Tick(object param)
        {
            if (m_IsRunningGameLoop) return;
            m_IsRunningGameLoop = true;
            try
            {
                await GameLoop();
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex.ToString());
                m_Logger.Error(ex.StackTrace);
            }
            finally
            {
                m_IsRunningGameLoop = false;
            }
            
        }

        public string GetGameReady()
        {
            if (m_GameReadyCountdown >= 0)
            {
                UpdateRoundState(GAME_STATUS.GetGameReady);
                m_Logger.Info("Getting ready - " + m_GameReadyCountdown);
                m_GameReadyCountdown--;
            }
            else
            {
                m_GameReadyCountdown = -1;
                //m_RoundStartTime = DateTime.Now;
                //m_GameState = GAME_STATUS.StartNewRound;
                UpdateRoundState(GAME_STATUS.StartNewRound);
            }
            return "";
        }

        public string StartNewRound()
        {
            m_Logger.Info("Start a new round...");

            m_RoundStartTime = DateTime.Now;

            m_RoundIndex++;

            m_PlayerCards.Clear();
            m_BankerCards.Clear();

            PlayerPoints = -1;
            BankerPoints = -1;

            m_Logger.Info("New round ID: " + GetGameId());

            m_GameReadyCountdown = -1;
            m_BettingTimeCountdown = BET_TIME_COUNTDOWN;
            //m_GameState = GAME_STATUS.BettingTime;
            UpdateRoundState(GAME_STATUS.BettingTime);

            m_Logger.Info("Start betting time...");

            return "";
        }

        public string StartBettingTime()
        {
            //m_Logger.Info("Start betting time...");

            if (m_BettingTimeCountdown >= 0)
            {
                UpdateRoundState(GAME_STATUS.BettingTime);
                m_Logger.Info("Betting time - " + m_BettingTimeCountdown);
                m_BettingTimeCountdown--;
            }
            else
            {
                m_BettingTimeCountdown = -1;
                //m_GameState = GAME_STATUS.DealingFirst4Cards;
                UpdateRoundState(GAME_STATUS.DealingFirst4Cards);
            }
            return "";
        }

        public string DealingFirst4Cards()
        {
            m_Logger.Info("Dealing first 4 cards...");

            if (m_WaitingForDealing) return "";

            try
            {
                m_WaitingForDealing = true;
                Task.Run(async () =>
                {
                    try
                    {
                        //System.Diagnostics.Debugger.Break();

                        //var remoteServices = m_Node.GetRemoteServices();
                        //foreach (var key in remoteServices.Keys) Console.WriteLine(key);

                        string card1 = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(), "dealing", "cards", "1");
                        if (String.IsNullOrEmpty(card1)) throw new Exception("Table controller no response.");
                        m_PlayerCards.Add(card1);

                        string card2 = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(), "dealing", "cards", "1");
                        if (String.IsNullOrEmpty(card2)) throw new Exception("Table controller no response.");
                        m_BankerCards.Add(card2);

                        string card3 = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(), "dealing", "cards", "1");
                        if (String.IsNullOrEmpty(card3)) throw new Exception("Table controller no response.");
                        m_PlayerCards.Add(card3);

                        string card4 = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(), "dealing", "cards", "1");
                        if (String.IsNullOrEmpty(card4)) throw new Exception("Table controller no response.");
                        m_BankerCards.Add(card4);

                        m_WaitingForDealing = false;

                        //m_GameState = GAME_STATUS.DealingLastPlayerCard;
                        UpdateRoundState(GAME_STATUS.DealingLastPlayerCard);
                    }
                    catch(Exception ex)
                    {
                        m_PlayerCards.Clear();
                        m_BankerCards.Clear();
                        m_WaitingForDealing = false;

                        Console.WriteLine("DealingFirst4Cards Error - " + ex.ToString());
                    }
                    

                });
            }
            catch (Exception ex)
            {
                m_PlayerCards.Clear();
                m_BankerCards.Clear();
                m_WaitingForDealing = false;

                Console.WriteLine("DealingFirst4Cards Error - " + ex.ToString());
            }
            
            return "";
        }

        private void DealingOnePlayerCard(GAME_STATUS nextState = GAME_STATUS.Unknown)
        {
            if (m_WaitingForDealing) return;
            try
            {
                m_WaitingForDealing = true;
                Task.Run(async () =>
                {
                    try
                    {
                        string card1 = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(), "dealing", "cards", "1");
                        if (String.IsNullOrEmpty(card1)) throw new Exception("Table controller no response.");
                        else m_PlayerCards.Add(card1);
                    }
                    catch (Exception ex)
                    {
                        m_WaitingForDealing = false;
                        Console.WriteLine("DealingOnePlayerCard Error - " + ex.ToString());
                        return;
                    }

                    m_WaitingForDealing = false;

                    //if (nextState != GAME_STATUS.Unknown) m_GameState = nextState;
                    if (nextState != GAME_STATUS.Unknown) UpdateRoundState(nextState);
                });
            }
            catch (Exception ex)
            {
                m_WaitingForDealing = false;
                Console.WriteLine("DealingOnePlayerCard Error - " + ex.ToString());
            }
        }

        private void DealingOneBankerCard(GAME_STATUS nextState = GAME_STATUS.Unknown)
        {
            if (m_WaitingForDealing) return;
            try
            {
                m_WaitingForDealing = true;
                Task.Run(async () =>
                {
                    try
                    {
                        string card1 = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(), "dealing", "cards", "1");
                        if (String.IsNullOrEmpty(card1)) throw new Exception("Table controller no response.");
                        m_BankerCards.Add(card1);
                    }
                    catch (Exception ex)
                    {
                        m_WaitingForDealing = false;
                        Console.WriteLine("DealingOneBankerCard Error - " + ex.ToString());
                        return;
                    }

                    m_WaitingForDealing = false;

                    //if (nextState != GAME_STATUS.Unknown) m_GameState = nextState;
                    if (nextState != GAME_STATUS.Unknown) UpdateRoundState(nextState);
                });
            }
            catch (Exception ex)
            {
                m_WaitingForDealing = false;
                Console.WriteLine("DealingOneBankerCard Error - " + ex.ToString());
            }
        }

        public string DealingLastPlayerCard()
        {
            m_Logger.Info("Dealing last player card (if necessary) ...");

            if (m_WaitingForDealing) return "";

            if (m_PlayerCards.Count != 2 || m_BankerCards.Count != 2) throw new Exception("error step");

            int playerCard1 = GetCardValue(m_PlayerCards[0][0].ToString());
            int playerCard2 = GetCardValue(m_PlayerCards[1][0].ToString());
            int playerValue = (playerCard1 + playerCard2) % 10;

            if (playerValue >= 0 && playerValue <= 5)
            {
                DealingOnePlayerCard(GAME_STATUS.DealingLastBankerCard);
            }
            else
            {
                m_WaitingForDealing = false;
                //m_GameState = GAME_STATUS.DealingLastBankerCard;
                UpdateRoundState(GAME_STATUS.DealingLastBankerCard);
            }
            
            return "";
        }

        public string DealingLastBankerCard()
        {
            m_Logger.Info("Dealing last banker card (if necessary) ...");

            if (m_WaitingForDealing) return "";

            int bankerCard1 = GetCardValue(m_BankerCards[0][0].ToString());
            int bankerCard2 = GetCardValue(m_BankerCards[1][0].ToString());
            int bankerValue = (bankerCard1 + bankerCard2) % 10;

            if (bankerValue >= 0 && bankerValue <= 2)
            {
                DealingOneBankerCard(GAME_STATUS.CountingPoints);
            }
            else if (bankerValue == 3)
            {
                bool needMore = true;
                if (m_PlayerCards.Count == 3)
                {
                    int playerCard3 = GetCardValue(m_PlayerCards[2][0].ToString());
                    if (playerCard3 == 8) needMore = false;
                }
                if (needMore) DealingOneBankerCard(GAME_STATUS.CountingPoints);
                //else m_GameState = GAME_STATUS.CountingPoints;
                else UpdateRoundState(GAME_STATUS.CountingPoints);
            }
            else if (bankerValue == 4)
            {
                bool needMore = true;
                if (m_PlayerCards.Count == 3)
                {
                    int playerCard3 = GetCardValue(m_PlayerCards[2][0].ToString());
                    if (playerCard3 == 0 || playerCard3 == 1 || playerCard3 == 8 || playerCard3 == 9) needMore = false;
                }
                if (needMore) DealingOneBankerCard(GAME_STATUS.CountingPoints);
                //else m_GameState = GAME_STATUS.CountingPoints;
                else UpdateRoundState(GAME_STATUS.CountingPoints);
            }
            else if (bankerValue == 5)
            {
                bool needMore = true;
                if (m_PlayerCards.Count == 3)
                {
                    int playerCard3 = GetCardValue(m_PlayerCards[2][0].ToString());
                    if (playerCard3 == 0 || playerCard3 == 1 || playerCard3 == 2 
                        || playerCard3 == 3 || playerCard3 == 8 || playerCard3 == 9) needMore = false;
                }
                if (needMore) DealingOneBankerCard(GAME_STATUS.CountingPoints);
                //else m_GameState = GAME_STATUS.CountingPoints;
                else UpdateRoundState(GAME_STATUS.CountingPoints);
            }
            else if (bankerValue == 6)
            {
                bool needMore = false;
                if (m_PlayerCards.Count == 3)
                {
                    int playerCard3 = GetCardValue(m_PlayerCards[2][0].ToString());
                    if (playerCard3 == 6 || playerCard3 == 7) needMore = true;
                }
                if (needMore) DealingOneBankerCard(GAME_STATUS.CountingPoints);
                //else m_GameState = GAME_STATUS.CountingPoints;
                else UpdateRoundState(GAME_STATUS.CountingPoints);
            }
            else
            {
                m_WaitingForDealing = false;
                //m_GameState = GAME_STATUS.CountingPoints;
                UpdateRoundState(GAME_STATUS.CountingPoints);
            }

            return "";
        }

        public void CountingPoints()
        {
            m_Logger.Info("Counting points...");
            PlayerPoints = GetPlayerPoints();
            BankerPoints = GetBankerPoints();

            int result = 0;
            if (PlayerPoints < BankerPoints) result = 1;
            if (PlayerPoints > BankerPoints) result = 2;
            if (PlayerPoints == BankerPoints) result = 3;

            result = result * 100 + BankerPoints * 10 + PlayerPoints;
            m_History.Enqueue(result);
            if (m_History.Count > MAX_HIST_LENGTH) m_History.Dequeue();

            //m_GameState = GAME_STATUS.OutputGameResult;
            UpdateRoundState(GAME_STATUS.OutputGameResult);

        }

        public async Task OutputGameResult()
        {
            m_Logger.Info("Player: " + String.Join(",", m_PlayerCards.ToArray()) + " = " + PlayerPoints);
            m_Logger.Info("Banker: " + String.Join(",", m_BankerCards.ToArray()) + " = " + BankerPoints);
            m_Logger.Info("Final game result - Player: " + PlayerPoints + "  |  Banker: " + BankerPoints);
            if (PlayerPoints > BankerPoints) m_Logger.Info("PLAYER WIN");
            if (PlayerPoints < BankerPoints) m_Logger.Info("BANKER WIN");
            if (PlayerPoints == BankerPoints) m_Logger.Info("TIE");
            m_GameReadyCountdown = GET_READY_COUNTDOWN;
            //m_GameState = GAME_STATUS.GetGameReady;

            UpdateRoundState(GAME_STATUS.GetGameReady);

            //System.Diagnostics.Debugger.Break();

            m_Logger.Info("Settling...");
            string replystr = await RemoteCaller.RandomCall(m_Node.GetRemoteServices(), "check-bet", "check", m_Node.GetName());
            if (!replystr.Contains('[')) m_Logger.Error("Failed to update bets by game result - " + replystr);
            else
            {
                dynamic reply = m_Node.GetJsonHelper().ToJsonObject(replystr);
                Stack<dynamic> bets = new Stack<dynamic>();
                foreach (var item in reply) bets.Push(item);

                int batch = 0;
                while (bets.Count > 0)
                {
                    batch++;
                    m_Logger.Info("Sending settle request - " + batch);

                    List<dynamic> list = new List<dynamic>();
                    int count = bets.Count >= 5 ? 5 : bets.Count;
                    for (var i = 1; i <= count; i++) list.Add(bets.Pop());
                    await Task.Run(() => RemoteCaller.RandomCall(m_Node.GetRemoteServices(), "settle-bet", "settle", m_Node.GetJsonHelper().ToJsonString(list)));
                }

                m_Logger.Info("Settle done");
            }
            
        }

        public int GetPlayerPoints()
        {
            int points = 0;
            foreach (var card in m_PlayerCards)
            {
                points += GetCardValue(card[0].ToString());
            }
            return points % 10;
        }

        public int GetBankerPoints()
        {
            int points = 0;
            foreach (var card in m_BankerCards)
            {
                points += GetCardValue(card[0].ToString());
            }
            return points % 10;
        }

        /*
        public List<string> PlayOneRound()
        {
            var lines = new List<string>();

            StartNewRound();
            lines.Add("Start a new round...");

            DealingFirst4Cards();
            lines.Add("Dealing first 4 cards...");
            lines.Add("Player: " + String.Join(",", m_PlayerCards.ToArray()) + " = " + GetPlayerPoints());
            lines.Add("Banker: " + String.Join(",", m_BankerCards.ToArray()) + " = " + GetBankerPoints());

            lines.Add("Dealing last 2 cards (if necessary) ...");
            DealingLastPlayerCard();
            DealingLastBankerCard();
            lines.Add("Player: " + String.Join(",", m_PlayerCards.ToArray()) + " = " + GetPlayerPoints());
            lines.Add("Banker: " + String.Join(",", m_BankerCards.ToArray()) + " = " + GetBankerPoints());

            int playerPoints = GetPlayerPoints();
            int bankerPoints = GetBankerPoints();
            lines.Add("Final game result - Player: " + playerPoints + "  |  Banker: " + bankerPoints);
            if (playerPoints > bankerPoints) lines.Add("PLAYER WIN");
            if (playerPoints < bankerPoints) lines.Add("BANKER WIN");
            if (playerPoints == bankerPoints) lines.Add("TIE");

            return lines;
        }
        */
    }
}
