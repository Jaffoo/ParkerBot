using ParkerBot;

namespace Helper
{
    public class AliYun
    {
        private string? _path;
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
            HttpClient httpClient = new();
            return await httpClient.GetStringAsync("http://127.0.0.1:5555/ali/upload?path=" + _path + "&album=" + Const.ConfigModel.BD.albumName);
        }

        public async Task<string> GetList()
        {
            HttpClient httpClient = new();
            return await httpClient.GetStringAsync("http://127.0.0.1:5555/ali/getalbumphotos?album=" + Const.ConfigModel.BD.albumName);
        }
    }
}
