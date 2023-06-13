using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParkerBot;

namespace Helper
{
    public class Baidu
    {
        public static readonly string appKey = Const.ConfigModel.BD.appKey ?? "";
        public static readonly string appSeret = Const.ConfigModel.BD.appSeret ?? "";
        public static string GetBaiduToken()
        {
            string authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new();
            List<KeyValuePair<string, string>> paraList = new()
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", "8zXHBKFPLNjOxUB1EavgVzgZ"),
                new KeyValuePair<string, string>("client_secret", "rXc2H9QwlLhrW9BRSdGpHBiye3I1jf9X")
            };

            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            var token = JObject.Parse(result)["access_token"]?.ToString() ?? "";
            return token;
        }

        public static async Task<int> IsFaceAndCount(string img)
        {
            if (!Const.EnableModule.bd) return 1;
            try
            {
                var url = img;
                img = await Base64.UrlImgToBase64(img);
                if (string.IsNullOrWhiteSpace(img)) return 0;
                string token = GetBaiduToken();
                string host = "https://aip.baidubce.com/rest/2.0/face/v3/detect?access_token=" + token;
                string str = "{\"image\":\"" + img + "\",\"image_type\":\"BASE64\",\"face_type\":\"LIVE\",\"liveness_control\":\"LOW\"}";
                StringContent stringContent = new(str);
                HttpClient client = new();
                var response = await client.PostAsync(host, stringContent);
                var res = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(res);
                if (string.IsNullOrWhiteSpace(result["result"]?.ToString())) return 0;
                var obj = JObject.Parse(JsonConvert.SerializeObject(result["result"]));
                if (!obj.ContainsKey("face_num")) return 0;
                var faceNum = Convert.ToInt32(obj["face_num"]?.ToString() ?? "0");
                return faceNum;
            }
            catch (Exception e)
            {
                var _context = new LiteContext();
                await _context.Logs.AddAsync(new Logs
                {
                    message = "报错信息：\n" + e.Message + "\n堆栈信息：\n" + e.StackTrace,
                    createDate = DateTime.Now,
                });
                await _context.SaveChangesAsync();
                await _context.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
                return 0;
            }
        }

        public static async Task<float> FaceMatch(string img)
        {
            if (!Const.EnableModule.bd) return Weibo.Audit;
            try
            {
                img = await Base64.UrlImgToBase64(img);
                if (string.IsNullOrWhiteSpace(img)) return 0;
                var imageList = Const.ConfigModel.BD.imageList;
                if (string.IsNullOrWhiteSpace(imageList)) return 0;
                List<string> faceStandard = imageList.Split(",").ToList();
                List<float> scores = new();
                foreach (var item in faceStandard)
                {
                    string img64 = Base64.PathToBase64(Directory.GetCurrentDirectory() + "/wwwroot" + item);
                    if (string.IsNullOrWhiteSpace(img64)) return 0;
                    string token = GetBaiduToken();
                    string host = "https://aip.baidubce.com/rest/2.0/face/v3/match?access_token=" + token;

                    string str = "[{\"image\": \"" + img + "\", \"image_type\": \"BASE64\", \"face_type\": \"LIVE\", \"quality_control\": \"LOW\"}," +
         "{\"image\": \"" + img64 + "\", \"image_type\": \"BASE64\", \"face_type\": \"LIVE\", \"quality_control\": \"LOW\"}]";
                    StringContent stringContent = new(str);
                    HttpClient client = new();
                    var response = await client.PostAsync(host, stringContent);
                    var resultStr = await response.Content.ReadAsStringAsync();
                    var result = JObject.Parse(resultStr);
                    if (string.IsNullOrWhiteSpace(result["result"]?.ToString() ?? "")) continue;
                    if (string.IsNullOrWhiteSpace(result["result"]?.ToString() ?? "")) return 0;
                    var obj = (JObject)result["result"]!;
                    if (obj == null) return 0;
                    var score = float.Parse(obj["score"]?.ToString() ?? "0");
                    scores.Add(score);
                    await Task.Delay(1000);
                }
                return scores.Count == 0 ? Weibo.Audit : scores.Max();
            }
            catch (Exception e)
            {
                var _context = new LiteContext();
                await _context.Logs.AddAsync(new Logs
                {
                    message ="报错信息：\n"+ e.Message + "\n堆栈信息：\n" + e.StackTrace,
                    createDate = DateTime.Now,
                });
                await _context.SaveChangesAsync();
                await _context.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
                return 0;
            }
        }
    }
}