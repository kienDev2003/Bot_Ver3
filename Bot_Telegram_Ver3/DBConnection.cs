using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_Telegram_Ver3
{
    internal class DBConnection
    {
        private string strConn = ConfigurationManager.AppSettings["strConn"].ToString();
        public SQLiteConnection conn = null;

        public SQLiteConnection GetConn()
        {
            if(conn == null)
            {
                conn = new SQLiteConnection(strConn);
            }
            if(conn != null && conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            return conn;
        }

        public SQLiteConnection CloseConn()
        {
            if(conn != null && conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
            return conn;
        }
    }
}
