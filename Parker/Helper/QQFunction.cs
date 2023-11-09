using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace ParkerBot.Helper
{
    public class QQFunction
    {
        public static List<int> Enables => Const.ConfigModel.QQ.funcEnable.ToIntList();
        private static readonly HttpHelper _httpHelper = new();
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
            if (string.IsNullOrWhiteSpace(ChatGPTKey)) return "请配置ChatGPT密钥！";
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
