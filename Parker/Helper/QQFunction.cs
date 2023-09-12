using Manganese.Array;
using Mirai.Net.Data.Messages;
using NetDimension.NanUI.Browser.ResourceHandler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace ParkerBot.Helper
{
    public class QQFunction
    {
        public static readonly List<string> Keywrods = new() { "爬", "比心", "丢", "处对象" };
        public static List<int> Enables => Const.ConfigModel.QQ.funcEnable.ToIntList();
        private static readonly HttpHelper _httpHelper = new("https://xiaobai.klizi.cn/API");
        public static string ChatGPTKey => Const.ConfigModel.QQ.gptKey;
        public static Dictionary<string, List<object>> LastMsg = new();
        #region chatgpt3.5
        /// <summary>
        /// chatgpt
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public static async Task<string> ChatGPT(string question, string qq)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question)) return "请输入问题！";
                var url = "https://api.chatanywhere.cn/v1/chat/completions";
                var objs = new List<object>();
                if (LastMsg.ContainsKey(qq)) objs.AddRange(LastMsg[qq]);
                objs.Add(new
                {
                    role = "user",
                    content = question
                });
                var obj = new
                {
                    model = "gpt-3.5-turbo",
                    messages = objs
                };
                var body = JsonConvert.SerializeObject(obj);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Content = new StringContent(body)
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ChatGPTKey}");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Apifox/1.0.0 (https://apifox.com)");

                var response = await client.SendAsync(request);
                var res = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(res);
                StringBuilder str = new();
                if (data.ContainsKey("choices"))
                {
                    if (data["choices"] != null)
                    {
                        var list = JArray.FromObject(data["choices"]!);
                        if (list.Count > 0)
                        {
                            foreach (JObject item in list)
                            {
                                str.Append(item["message"]!["content"]!.ToString());
                                if (LastMsg.ContainsKey(qq))
                                {
                                    if (LastMsg[qq].Count > 10) LastMsg[qq].Clear();
                                    LastMsg[qq].Add(new
                                    {
                                        role = "assistant",
                                        content = item["message"]!["content"]
                                    });
                                }
                                else
                                {
                                    LastMsg.Add(qq, new());
                                    LastMsg[qq].Add(new
                                    {
                                        role = "assistant",
                                        content = item["message"]!["content"]
                                    });
                                }
                            }
                        }
                    }
                }
                return str.ToString();
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

        #region 缩写查询
        /// <summary>
        /// 支持上下文
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public static async Task<string> Abbreviations(string words)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(words)) return "请输入问题！";
                string url = "https://api.pearktrue.cn/api/suoxie/?word=" + words;
                var response = await _httpHelper.GetAsync(url);
                var data = JObject.Parse(response);
                if (data["code"]!.ToString() == "200")
                {
                    return string.Join(",", data["data"] ?? new JArray());
                }
                if (data["code"]!.ToString() == "201")
                {
                    return data["msg"]!.ToString();
                }
                return data["msg"]?.ToString() ?? "";
            }
            catch (Exception)
            {
                return "";
            }
        }
        #endregion
    }
}
