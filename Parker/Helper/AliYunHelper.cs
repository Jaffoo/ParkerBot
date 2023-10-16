using ParkerBot;

namespace Helper
{
    public class AliYun
    {
        private readonly string? _path;
        /// <summary>
        /// 上传阿里云盘
        /// </summary>
        /// <param name="path">文件路径(用'/')</param>
        public AliYun(string path)
        {
            _path = path;
        }

        public async Task<string> Upload()
        {
            try
            {
                HttpClient httpClient = new();
                return await httpClient.GetStringAsync("http://127.0.0.1:5555/ali/upload?path=" + _path + "&album=" + Const.ConfigModel.BD.albumName);
            }
            catch (Exception e)
            {
                e.AddLog();
                return "{'msg':'" + e.Message + "'}";
            }
        }

        public static async Task<string> GetList()
        {
            try
            {
                HttpClient httpClient = new();
                return await httpClient.GetStringAsync("http://127.0.0.1:5555/ali/getalbumphotos?album=" + Const.ConfigModel.BD.albumName);

            }
            catch (Exception e)
            {
                e.AddLog();
                return "{'msg':'" + e.Message + "'}";
            }
        }
    }
}
