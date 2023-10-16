namespace ParkerBot
{
    public class SqlUpdate
    {
        public static async void ExecuteSql()
        {
            HttpClient client = new();
            var res = await client.GetStringAsync("https://gitee.com/jaffoo/ParkerBotV2/raw/master/Parker/wwwroot/sql/update.txt");
            var path = Directory.GetCurrentDirectory() + "/wwwroot/sql/update.txt";
            if (!File.Exists(path))
            {
                var stream = File.Create(path);
                stream.Close();
            }
            File.WriteAllText(path, res);
            var list = await File.ReadAllLinesAsync(path);
            foreach (var sql in list)
            {
                SqlHelper.ExecuteNonQuery(sql);
            }
        }
    }
}
