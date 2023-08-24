namespace ParkerBot
{
    public class SqlUpdate
    {
        public static void ExecuteSql()
        {
            HttpClient client = new HttpClient();
            var sqls = client.GetStringAsync("https://gitee.com/jaffoo/ParkerBotV2/raw/master/Parker/wwwroot/sql/update.sql").Result;
            SqlHelper.ExecuteNonQuery(sqls);
        }
    }
}
