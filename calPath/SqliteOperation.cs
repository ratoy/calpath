using System;
using System.Collections.Generic;
using System.Text;
//using System.Data.Sqlite;
using Mono.Data.Sqlite;
using System.Data;
using System.Threading;

namespace GenPath
{
    ///<summary>
    /// * Copyright(c)  GeoStar Soft Group
    /// * Description   : 
    /// * CreateDate    : 2014/12/17 17:49:27
    /// * Creator       : wuhao
    /// * LastChangeDate: 
    /// * LastChanger   : 
    /// * Version Info  : 1.0
    ///</summary>
    class SqliteOperation
    {
        string m_ConnectString = "";
        SqliteConnection m_Conn;
        SqliteDataAdapter m_DataAdapter;
        SqliteCommand m_Command;//
        string m_Passwd = "";
        string m_LastErrorMsg = "";
        bool m_Busy = false;
        Random m_Rd = new Random();
        int MAX_TIME_SEC = 30;

        public string LastErrorMsg
        {
            get { return m_LastErrorMsg; }
        }

        ///构造函数
        /// <summary>
        /// 构造sqlite对象
        /// </summary>
        /// <param name="SqliteDbName">包含路径和文件名</param>
        public SqliteOperation(string SqliteDbName)
        {
            if (!System.IO.File.Exists(SqliteDbName))
            {
                CreateSqliteDB(SqliteDbName, "");
            }
            ConnectionInit(SqliteDbName, "");
        }

        /// <summary>
        /// 构造sqlite对象
        /// </summary>
        /// <param name="SqliteDbName">包含路径和文件名</param>
        /// <param name="Passwd">密码</param>
        public SqliteOperation(string SqliteDbName, string Passwd)
        {
            if (!System.IO.File.Exists(SqliteDbName))
            {
                CreateSqliteDB(SqliteDbName, Passwd);
            }

            ConnectionInit(SqliteDbName, Passwd);
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="SqliteDbName"></param>
        /// <param name="Passwd"></param>
        void CreateSqliteDB(string SqliteDbName, string Passwd)
        {
            //创建文件j
            SqliteConnection.CreateFile(SqliteDbName);
            //初始化连接
            ConnectionInit(SqliteDbName, "");
            //设置密码
            ChangePasswd(Passwd);
        }

        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="SqliteDbName"></param>
        /// <param name="Passwd"></param>
        void ConnectionInit(string SqliteDbName, string Passwd)
        {
            m_LastErrorMsg = "";
            try
            {
                m_ConnectString = "Data Source=" + SqliteDbName;
                m_Conn = new SqliteConnection(m_ConnectString);
                m_Command = m_Conn.CreateCommand();
                m_Passwd = Passwd;
            }
            catch (Exception e)
            {
                m_LastErrorMsg = e.Message;
                System.Diagnostics.Debug.Print(e.Message);
                ShutDown();
            }
        }

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="Passwd"></param>
        /// <returns></returns>
        public bool ChangePasswd(string Passwd)
        {
            m_LastErrorMsg = "";
            try
            {
                ConnectToDB();
                m_Conn.ChangePassword(Passwd);
                ShutDown();
                return true;
            }
            catch (Exception e)
            {
                m_LastErrorMsg = e.Message;
                System.Diagnostics.Debug.Print(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <returns></returns>
        public bool TestDB()
        {
            if (ConnectToDB())
            {
                ShutDown();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 连接到数据库
        /// </summary>
        private bool ConnectToDB()
        {
            m_LastErrorMsg = "";
            try
            {
                if (m_Conn.State != ConnectionState.Open)
                {
                    m_Conn.SetPassword(m_Passwd);
                    m_Conn.Open();
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                m_LastErrorMsg = e.Message;
                System.Diagnostics.Debug.Print(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 关闭数据库
        /// </summary>
        private void ShutDown()
        {
            m_Busy = false;
            m_Conn.Close();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="QueryCmd">查询命令</param>
        /// <returns></returns>
        public DataTable Query(string QueryCmd)
        {
            //判断是否有其它线程正在操作
            SetExclusive();

            m_LastErrorMsg = "";
            if (!QueryCmd.TrimStart(' ').ToLower().StartsWith("select"))
            {
                m_LastErrorMsg = "查询命令不正确！";
                System.Diagnostics.Debug.Print(m_LastErrorMsg);
                return null;
            }
            //连接到数据库
            ConnectToDB();

            try
            {
                m_DataAdapter = new SqliteDataAdapter(QueryCmd, m_Conn);

                DataTable table = new DataTable();
                table.Locale = System.Globalization.CultureInfo.InvariantCulture;
                m_DataAdapter.Fill(table);

                //关闭数据库
                ShutDown();
                return table;
            }
            catch (Exception e)
            {
                ShutDown();
                m_LastErrorMsg = e.Message;
                System.Diagnostics.Debug.Print("数据查询出错！\r\n原因：" + e.Message);
                return null;
            }

        }

        /// <summary>
        /// 批量操作
        /// </summary>
        /// <param name="InstertCmdList"></param>
        /// <returns></returns>
        public bool BatProcess(List<string> BatCmdList)
        {
            if (BatCmdList == null || BatCmdList.Count == 0)
            {
                return true;
            }

            //判断是否有其它线程正在操作
            SetExclusive();

            m_LastErrorMsg = "";
            //连接
            ConnectToDB();

            SqliteTransaction transcation = m_Conn.BeginTransaction();
            try
            {
                int count = BatCmdList.Count + 1;
                for (int i = 1; i < count; i++)
                {
                    OutputDebugMsg(BatCmdList[i - 1]);

                    m_Command.CommandText = BatCmdList[i - 1];
                    m_Command.ExecuteNonQuery();
                }
                transcation.Commit();
            }
            catch (Exception e)
            {
                m_LastErrorMsg = e.Message;
                //回滚
                transcation.Rollback();

                //关闭
                ShutDown();
                return false;
            }

            //关闭
            ShutDown();
            return true;
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="InsertCmd"></param>
        /// <returns></returns>
        public bool InsertData(string InsertCmd)
        {
            if (!InsertCmd.TrimStart(' ').ToLower().StartsWith("insert"))
            {
                m_LastErrorMsg = "插入命令不正确！";
                System.Diagnostics.Debug.Print(m_LastErrorMsg);
                return false;
            }

            return ExecuteSql(InsertCmd);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="DeleteCmd"></param>
        /// <returns></returns>
        public bool DeleteData(string DeleteCmd)
        {
            if (!DeleteCmd.TrimStart(' ').ToLower().StartsWith("delete"))
            {
                m_LastErrorMsg = "删除命令不正确！";
                System.Diagnostics.Debug.Print(m_LastErrorMsg);
                return false;
            }

            return ExecuteSql(DeleteCmd);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="UpdateCmd"></param>
        /// <returns></returns>
        public bool UpdateData(string UpdateCmd)
        {
            if (!UpdateCmd.TrimStart(' ').ToLower().StartsWith("update"))
            {
                m_LastErrorMsg = "更新数据命令不正确！";
                System.Diagnostics.Debug.Print(m_LastErrorMsg);
                return false;
            }

            return ExecuteSql(UpdateCmd);
        }

        /// <summary>
        /// 执行sql命令
        /// </summary>
        /// <param name="SqlCmd"></param>
        /// <returns></returns>
        public bool ExecuteSql(string SqlCmd)
        {
            //判断是否有其它线程正在操作
            SetExclusive();

            OutputDebugMsg(SqlCmd);

            m_LastErrorMsg = "";
            //连接到数据库
            ConnectToDB();
            try
            {
                m_Command.CommandText = SqlCmd;
                m_Command.ExecuteNonQuery();
                //关闭数据库
                ShutDown();
            }
            catch (Exception e)
            {
                ShutDown();
                m_LastErrorMsg = e.Message;
                System.Diagnostics.Debug.Print("Sql语句执行失败！\r\n原因：" + e.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 压缩数据库
        /// </summary>
        public void CompressDB()
        {
            ExecuteSql("vacuum");
        }

        void SetExclusive()
        {
            DateTime dt = DateTime.Now;
            while (m_Busy)
            {
                //如果其它操作正在进行，延时
                Thread.Sleep(m_Rd.Next(1, 150));

                //如果超时了，也跳出
                if ((DateTime.Now - dt).TotalSeconds > MAX_TIME_SEC)
                {
                    break;
                }
            }
            m_Busy = true;
        }

        void OutputDebugMsg(string FunctionName)
        {
            //System.Diagnostics.Debug.Print("操作sqlite: " + FunctionName);
        }
    }
}
