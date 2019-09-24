using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MySharpServer.Common;

namespace MiniBaccarat.DataAccessServer.MerchantData
{
    public class MerchantDataCache
    {
        static CommonRng m_Rng = new CommonRng();

        private Timer m_Timer = null;

        private IServerNode m_Node = null;
        private IServerLogger m_Logger = null;

        private bool m_IsRunning = false;

        private string m_MainDatabase = "main";

        private string m_ServerName = "";

        private Dictionary<string, dynamic> m_Merchants = new Dictionary<string, dynamic>();

        public MerchantDataCache(IServerNode node)
        {
            m_Node = node;
            m_Logger = m_Node.GetLogger();

            m_ServerName = m_Node.GetName();

            m_IsRunning = false;
        }

        public void Start()
        {
            Stop();

            m_Timer = new Timer(Tick, m_Rng, 500, 1000 * 5); // update merchant data every 5s
        }

        public void Stop()
        {
            if (m_Timer != null)
            {
                Thread.Sleep(500);
                m_Timer.Dispose();
                m_Timer = null;
            }

        }

        private void Tick(object param)
        {
            if (m_IsRunning) return;
            m_IsRunning = true;
            try
            {
                UpdateLocalCacheFromDb();
            }
            catch (Exception ex)
            {
                m_Logger.Error(ex.ToString());
                m_Logger.Error(ex.StackTrace);
            }
            finally
            {
                m_IsRunning = false;
            }

        }

        private void UpdateLocalCacheFromDb()
        {
            Dictionary<string, dynamic> merchants = new Dictionary<string, dynamic>();

            var dbhelper = m_Node.GetDataHelper();
            using (var cnn = dbhelper.OpenDatabase(m_MainDatabase))
            {
                using (var cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = " select * from db_baccarat_main.tbl_merchant_info ";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new
                            {
                                merchant = reader["merchant_code"].ToString(),
                                url = reader["api_url"].ToString(),
                                active = Convert.ToInt32(reader["is_active"].ToString())
                            };

                            if (merchants.ContainsKey(item.merchant)) merchants.Remove(item.merchant);
                            merchants.Add(item.merchant, item);

                        }
                    }
                }
            }

            m_Merchants = merchants;
        }

        public string GetMerchantUrl(string merchant)
        {
            var url = "";
            var merchants = m_Merchants;

            if (merchants.ContainsKey(merchant))
            {
                var item = merchants[merchant];
                if (item.active > 0) url = item.merchant;
            }

            return url;
        }
    }
}
