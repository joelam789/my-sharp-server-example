using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;

namespace MiniBaccarat.SimpleClient
{
    public partial class MainForm : Form
    {
        WebSocket m_Socket = null;

        CommonRng m_Rng = new CommonRng();

        string m_FrontEndServerURL = "ws://127.0.0.1:9996";
        string m_BettingServerURL = "http://127.0.0.1:9998";

        public MainForm()
        {
            InitializeComponent();
        }

        public static async Task<WebResponse> TryToGetResponse(HttpWebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("HttpWebRequest");
            }

            WebResponse response = null;

            try
            {
                response = await request.GetResponseAsync();
            }
            catch (WebException ex)
            {
                response = ex.Response;
                if (response == null) throw;
            }

            return response;
        }

        public void LogMsg(string text)
        {
            BeginInvoke((Action)(() =>
            {
                mmLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text + Environment.NewLine);
                mmLog.SelectionStart = mmLog.Text.Length;
                mmLog.ScrollToCaret();
            }));
        }

        public void LogMsg2(string text)
        {
            BeginInvoke((Action)(() =>
            {
                mmLog2.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text + Environment.NewLine);
                mmLog2.SelectionStart = mmLog2.Text.Length;
                mmLog2.ScrollToCaret();
            }));
        }

        private void WhenConnect(object sender)
        {
            LogMsg("Connected to server - " + m_FrontEndServerURL);
        }

        private void WhenGetError(string errmsg)
        {
            Console.WriteLine("Socket Error: " + errmsg);
        }

        private void WhenDisconnect(object sender)
        {
            LogMsg("Disconnected from server - " + m_FrontEndServerURL);
        }

        private void WhenGetMessage(string msg)
        {
            //LogMsg("MESSAGE - " + msg);

            dynamic obj = JsonConvert.DeserializeObject(msg);
            if (obj.msg == "table_info")
            {
                LogMsg("MESSAGE - " + msg);
                BeginInvoke((Action)(() =>
                {
                   if (((JArray)obj.tables).Count > 0)
                   {
                        edtGameServer.Text = obj.tables[0].server;
                        edtTableCode.Text = obj.tables[0].table;
                        edtShoeCode.Text = obj.tables[0].shoe;
                        edtRound.Text = obj.tables[0].round;
                   }
                }));
            }
            if (obj.msg == "client_info")
            {
                LogMsg2("MESSAGE - " + msg);
                BeginInvoke((Action)(() =>
                {
                    edtFrontEnd.Text = obj.front_end;
                    edtClientId.Text = obj.client_id;
                }));
            }
            if (obj.msg == "bet_result")
            {
                foreach (var betResult in obj.results)
                {
                    if (betResult.payout > 0) LogMsg2("WIN - " + msg);
                    else LogMsg2("LOSE - " + msg);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            m_Socket = new WebSocket(m_FrontEndServerURL);
            m_Socket.AllowUnstrustedCertificate = true;
            m_Socket.NoDelay = true;
            m_Socket.EnableAutoSendPing = false;

            m_Socket.Closed += (sender1, e1) => { WhenDisconnect(sender1); };
            m_Socket.Opened += (sender1, e1) => { WhenConnect(sender1); };
            m_Socket.Error += (sender1, e1) => { WhenGetError(e1.Exception.Message); };
            m_Socket.MessageReceived += (sender1, e1) => { WhenGetMessage(e1.Message); };
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            m_Socket.Open();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            m_Socket.Close();
        }

        private async void btnPlaceBet_Click(object sender, EventArgs e)
        {
            var url = m_BettingServerURL + "/accept-bet/accept";
            var merchant = (m_Rng.Next(2) + 1).ToString();
            var player = merchant + m_Rng.Next(10).ToString() + m_Rng.Next(10).ToString() + m_Rng.Next(10).ToString();
            var req = new
            {
                server_code = edtGameServer.Text,
                table_code = edtTableCode.Text,
                shoe_code = edtShoeCode.Text,
                round_number = Convert.ToInt32(edtRound.Text),
                client_id = edtClientId.Text,
                front_end = edtFrontEnd.Text,

                merchant_code = "m" + merchant,
                player_id = "p" + player,

                bet_pool = cbbBetPool.Items.IndexOf(cbbBetPool.Text) + 1,
                bet_amount = Convert.ToInt32(cbbBetAmount.Text)
            };

            string input = JsonConvert.SerializeObject(req);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.Accept = "*/*";
            httpWebRequest.UserAgent = "curl/7.50.0";
            httpWebRequest.ContentType = "text/plain";
            httpWebRequest.Method = "POST";

            //httpWebRequest.Timeout = timeout > 0 ? timeout : DefaultTimeout;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                await streamWriter.WriteAsync(input);
                await streamWriter.FlushAsync();
                streamWriter.Close();
            }

            var ret = "";

            using (var response = await TryToGetResponse(httpWebRequest))
            {
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    ret = await streamReader.ReadToEndAsync();
                    streamReader.Close();
                }
            }

            LogMsg2("PLACE-BET REPLY - " + ret);
        }
    }

    public class CommonRng
    {
        private RNGCryptoServiceProvider m_rngsp = new RNGCryptoServiceProvider();

        // generate a random integer (0 <= value)
        public int Next()
        {
            byte[] rb = { 0, 0, 0, 0 };
            m_rngsp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0);
            return value < 0 ? -value : value;
        }

        // generate a random integer, less than the maximum value (0 <= value < max)
        public int Next(int max)
        {
            if (max <= 0) return 0;
            byte[] rb = { 0, 0, 0, 0 };
            m_rngsp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0) % max;
            return value < 0 ? -value : value;
        }

        // generate a random integer, between the minimum value and the maximum value (min <= value < max)
        public int Next(int min, int max)
        {
            return Next(max - min) + min;
        }
    }
}
