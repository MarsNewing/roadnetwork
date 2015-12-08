using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;

namespace RoadNetworkSystem.ADO.Access
{
    class AccessHelper
    {
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="Conn"></param>数据库连接
        /// <param name="mdbPath"></param>mdb数据库的路径
        /// Create by niuzhm
        public static OleDbConnection OpenConnection(string mdbPath)
        {
            OleDbConnection Conn;
            string conn_str = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbPath + "; Persist Security Info=False";

            Conn = new OleDbConnection(conn_str);
            try
            {
                Conn.Open();
                return Conn;
            }
            catch (Exception e)
            { 
                throw new Exception(e.Message);
            }
        }
    }
}
