using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace ParkerBot
{
    public static class SqlHelper
    {
        ///创建数据库文件
        public static bool CreateDBFile(string fileName)
        {
            string path = Environment.CurrentDirectory + @"/data/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string databaseFileName = path + fileName;
            if (!File.Exists(databaseFileName))
            {
                SQLiteConnection.CreateFile(databaseFileName);
                return true;
            }
            return false;
        }

        //生成连接字符串
        private static string CreateConnectionString()
        {
            SQLiteConnectionStringBuilder connectionString = new();
            connectionString.DataSource = @"data/config.db";

            string conStr = connectionString.ToString();
            return conStr;
        }

        /// <summary>
        /// 对插入到数据库中的空值进行处理
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object ToDbValue(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// 执行非查询的数据库操作
        /// </summary>
        /// <param name="sqlString">要执行的sql语句</param>
        /// <param name="parameters">参数列表</param>
        /// <returns>返回受影响的条数</returns>
        public static int ExecuteNonQuery(string sqlString, params SQLiteParameter[] parameters)
        {
            try
            {
                string connectionString = CreateConnectionString();
                using SQLiteConnection conn = new(connectionString);
                conn.Open();
                using SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlString;
                foreach (SQLiteParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                return cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 执行查询并返回查询结果第一行第一列
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sqlString, params SQLiteParameter[] parameters)
        {
            try
            {
                string connectionString = CreateConnectionString();
                using SQLiteConnection conn = new SQLiteConnection(connectionString);
                conn.Open();
                using SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlString;
                foreach (SQLiteParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                return cmd.ExecuteScalar();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 返回一行数据
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>DataRow</returns>
        public static JObject Query(string cmdText, params SQLiteParameter[] parameters)
        {
            try
            {
                var dt = GetDataTable(cmdText, parameters);
                if (dt.Rows.Count > 0)
                {
                    var dic = new Dictionary<object, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        var value = dt.Rows[0][col.ColumnName];
                        dic.Add(col.ColumnName, value);
                    }
                    return JObject.FromObject(dic);
                }
                return new JObject();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 查询多条数据
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="parameters">参数列表</param>
        /// <returns>返回查询的数据表</returns>
        public static DataTable GetDataTable(string sqlString, params SQLiteParameter[] parameters)
        {
            try
            {
                string connectionString = CreateConnectionString();
                using SQLiteConnection conn = new(connectionString);
                conn.Open();
                using SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlString;
                foreach (SQLiteParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                DataSet ds = new();
                SQLiteDataAdapter adapter = new(cmd);
                adapter.Fill(ds);
                conn.Close();
                return ds.Tables[0];
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 查询多条数据
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="parameters">参数列表</param>
        /// <returns>返回查询的数据表</returns>
        public static List<object> GetList(string sqlString, params SQLiteParameter[] parameters)
        {
            try
            {
                string connectionString = CreateConnectionString();
                using SQLiteConnection conn = new SQLiteConnection(connectionString);
                conn.Open();
                using SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlString;
                foreach (SQLiteParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                DataSet ds = new();
                SQLiteDataAdapter adapter = new(cmd);
                adapter.Fill(ds);
                conn.Close();
                var dt = ds.Tables[0];
                List<object> dic = new();
                foreach (DataRow row in dt.Rows)
                {
                    Dictionary<string, object> drow = new();
                    foreach (DataColumn col in dt.Columns)
                    {
                        var value = row[col.ColumnName];
                        var b = col.DefaultValue == value;
                        if (b)
                        {
                            if (col.DataType.Name.ToLower().Contains("int") || col.DataType.Name.ToLower().Contains("real")) value = 0;
                            if (col.DataType.Name.ToLower().Contains("string")) value = "";
                            if (col.DataType.Name.ToLower().Contains("date")) value = DateTime.Now;
                        }
                        drow.Add(col.ColumnName, value);
                    }
                    dic.Add(drow);
                }
                return dic;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 查询多条数据
        /// </summary>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="parameters">参数列表</param>
        /// <returns>返回查询的数据表</returns>
        public static List<T> GetList<T>(string sqlString, params SQLiteParameter[] parameters) where T : new()
        {
            try
            {
                string connectionString = CreateConnectionString();
                using SQLiteConnection conn = new(connectionString);
                conn.Open();
                using SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlString;
                foreach (SQLiteParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
                DataSet ds = new();
                SQLiteDataAdapter adapter = new(cmd);
                adapter.Fill(ds);
                conn.Close();
                var dt = ds.Tables[0];
                // 定义集合    
                List<T> ts = new();

                // 获得此模型的类型   
                Type type = typeof(T);
                string tempName = "";
                foreach (DataRow dr in dt.Rows)
                {
                    T t = new();
                    // 获得此模型的公共属性      
                    PropertyInfo[] propertys = t.GetType().GetProperties();
                    foreach (PropertyInfo pi in propertys)
                    {
                        tempName = pi.Name;  // 检查DataTable是否包含此列    

                        if (dt.Columns.Contains(tempName))
                        {
                            // 判断此属性是否有Setter      
                            if (!pi.CanWrite) continue;

                            object value = dr[tempName];
                            if (value != DBNull.Value)
                                pi.SetValue(t, value, null);
                        }
                    }
                    ts.Add(t);
                }
                return ts;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
