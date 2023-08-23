using Newtonsoft.Json.Linq;
using System.Web;

namespace ParkerBot.Helper
{
    public class QQFunction
    {
        public static readonly List<string> Keywrods = new() { "爬", "比心", "丢", "处对象" };
        public static List<int> Enables => Const.ConfigModel.QQ.funcEnable.ToIntList();
        private static readonly HttpHelper _httpHelper = new("https://xiaobai.klizi.cn/API");

        #region chatgpt3.5
        public static async Task<string> ChatGPT(string question)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question)) return "请输入问题！";
                string url = "/other/gpt.php?msg=" + HttpUtility.UrlEncode(question);
                var response = await _httpHelper.GetAsync(url);
                return response ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }
        #endregion

        #region 小爱闲聊
        public static async Task<string> XiaoAi(string question)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question)) return "请输入问题！";
                string url = "/other/xiaoai.php?data=&msg=" + HttpUtility.UrlEncode(question);
                var response = await _httpHelper.GetAsync(url);
                var res = JObject.Parse(response);
                return res["text"]?.ToString() ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }
        #endregion

        #region at作图
        public static async Task<string> AtPic(string qq, string keyword)
        {
            if (string.IsNullOrWhiteSpace(qq)) return "请输入QQ！";
            string url = GetUrl(keyword) + "?qq=" + qq;
            var response = await _httpHelper.GetBufferAsync(url);
            var res = Convert.ToBase64String(response);
            return res;
        }

        private static string GetUrl(string keyword)
        {
            switch (keyword)
            {
                case "爬": return "/ce/paa.php";
                case "比心": return "/ce/xin.php";
                case "处对象": return "/ce/xie.php";
                case "丢": return "/ce/diu.php";
                default: return "";
            }
        }
        #endregion

        #region 天气查询
        public static async Task<string> Weather(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return "请输入城市！";
            string url = "/other/qq_weather.php?data=&msg=" + HttpUtility.UrlEncode(city);
            var response = await _httpHelper.GetAsync(url);
            return response;
        }
        #endregion

        #region 文案搜索
        public static async Task<string> WenAn(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return "请输入城市！";
            string url = "/other/wenan.php?msg=" + HttpUtility.UrlEncode(keyword);
            var response = await _httpHelper.GetAsync(url);
            return response;
        }
        #endregion

        #region 舔狗
        public static async Task<string> Dog()
        {
            string url = "/other/tgrj.php";
            var response = await _httpHelper.GetAsync(url);
            return response;
        }
        #endregion
    }
}
