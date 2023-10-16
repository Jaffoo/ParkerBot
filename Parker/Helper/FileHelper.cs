using Helper;
using Mirai.Net.Sessions.Http.Managers;
using Newtonsoft.Json.Linq;
using ParkerBot;

namespace Helper
{
    public class FileHelper
    {
        public static bool SaveAliyunDisk
        {
            get { return Const.ConfigModel.BD.saveAliyunDisk; }
        }
        public static bool Save(string url)
        {
            try
            {
                HttpClient client = new();
                byte[] bytes = client.GetByteArrayAsync(url).Result;
                var root = Directory.GetCurrentDirectory() + @"/wwwroot/images/";
                if(!Directory.Exists(root)) Directory.CreateDirectory(root);
                var path = root + "66-" + DateTime.Now.ToString("yyMMddHHmmssfff") + ".jpeg";
                File.WriteAllBytes(path, bytes);
                if (SaveAliyunDisk)
                {
                    ThreadStart ts = new(async () =>
                    {
                        //存入阿里云盘
                        AliYun aliCloud = new(path);
                        var res = await aliCloud.Upload();
                        var data = JObject.Parse(res);
                        var msg = data["msg"]?.ToString() ?? "";
                        if (string.IsNullOrWhiteSpace(msg)) msg = "保存状态未知，请前往云盘查看!";
                        await Msg.SendAdminMsg(msg);
                    });
                    Thread t = new(ts);
                    t.Start();
                }
                return true;
            }
            catch (Exception e)
            {
                e.AddLog();
                return false;
            }
        }
    }
}
