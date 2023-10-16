using ParkerBot;

namespace Helper
{
    public class Base64Helper
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
                e.AddLog();
                return "";
            }
        }

        public static string PathToBase64(string path)
        {
            using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] bt = new byte[fs.Length];
            fs.Read(bt, 0, bt.Length);
            var str = Convert.ToBase64String(bt);
            fs.Close();
            return str;
        }
    }
}
