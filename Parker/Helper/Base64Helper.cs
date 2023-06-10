using ParkerBot;

namespace Helper
{
    public class Base64
    {
        public static async Task<string> UrlImgToBase64(string url)
        {
            try
            {
                HttpClient client = new();
                byte[] bytes = await client.GetByteArrayAsync(url);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception e)
            {
                LiteContext _context = new();
                _context.Logs.Add(new Logs
                {
                    message = e.ToString(),
                    createDate = DateTime.Now,
                });
                _context.SaveChanges();
                _context.Dispose();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
                return "";
            }
        }

        public static string PathToBase64(string path)
        {
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] bt = new byte[fs.Length];
            fs.Read(bt, 0, bt.Length);
            var str = Convert.ToBase64String(bt);
            fs.Close();
            return str;
        }
    }
}
