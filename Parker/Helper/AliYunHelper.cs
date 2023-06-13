using ParkerBot;
using System.Data.SQLite;

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
            try
            {
                HttpClient httpClient = new();
                return await httpClient.GetStringAsync("http://127.0.0.1:5555/ali/upload?path=" + _path + "&album=" + Const.ConfigModel.BD.albumName);
            }
            catch (Exception e)
            {
                var _liteContext = new LiteContext();
                await _liteContext.Logs.AddAsync(new()
                {
                    message = "报错信息：\n" + e.Message + "\n堆栈信息：\n" + e.StackTrace,
                    createDate = DateTime.Now,
                });
                await _liteContext.SaveChangesAsync();
                await _liteContext.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
                return "{'msg':'" + e.Message + "'}";
            }
        }

        public async Task<string> GetList()
        {
            try
            {
                HttpClient httpClient = new();
                return await httpClient.GetStringAsync("http://127.0.0.1:5555/ali/getalbumphotos?album=" + Const.ConfigModel.BD.albumName);

            }
            catch (Exception e)
            {
                var _liteContext = new LiteContext();
                await _liteContext.Logs.AddAsync(new()
                {
                    message = "报错信息：\n" + e.Message + "\n堆栈信息：\n" + e.StackTrace,
                    createDate = DateTime.Now,
                });
                await _liteContext.SaveChangesAsync();
                await _liteContext.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
                return "{'msg':'" + e.Message + "'}";
            }
        }
    }
}
