using Mirai.Net.Utils.Scaffolds;
using Newtonsoft.Json.Linq;
using ParkerBot;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Helper
{
    public class Pocket
    {
        public static LiteContext? _liteContext { get; set; }
        private static List<string> MsgType => Const.ConfigModel.KD.msgType.ToListV2();
        public static async Task PocketMessageReceiver(string str)
        {
            try
            {
                if (!Const.EnableModule.kd) return;
                _liteContext = new();
                var result = JObject.Parse(str);
                var time = result["time"]!.ToString();
                var channelName = result["channelName"]!.ToString();
                var name = result["ext"]!["user"]!["nickName"]!.ToString();
                int roleId = result["ext"]!["user"]!["roleId"]!.ToString().ToInt();
                string msgType = result["type"]!.ToString();
                string msbBody = "";

                if (roleId != 3) return;
                MessageChainBuilder mcb = new();
                mcb.Plain($"【{Const.ConfigModel.KD.name}|{channelName}】\n【{time}】\n{name}:");
                //图片
                if (msgType == "image")
                {
                    msbBody = result["attach"]!["url"]!.ToString();
                    await Task.Run(async () =>
                    {
                        await Weibo.FatchFace(msbBody);
                    });
                    if (!MsgType.Contains(msgType)) return;
                    mcb.ImageFromUrl(msbBody);
                }
                //文字
                else if (MsgType.Contains(msgType) && msgType == "text")
                {
                    //"230226137"
                    msbBody = result["body"]!.ToString();
                    mcb.Plain(msbBody);
                }
                //视频
                else if (MsgType.Contains(msgType) && msgType == "video")
                {
                    mcb.Plain(result["attach"]!["url"]!.ToString());
                }
                //语言
                else if (MsgType.Contains(msgType) && msgType == "audio")
                {
                    mcb.VoiceFromUrl(result["attach"]!["url"]!.ToString());
                }
                else if (msgType == "custom")
                {
                    var attach = result["attach"]!;
                    var messageType = attach["messageType"]!.ToString();
                    //回复
                    if (MsgType.Contains(messageType) && messageType == "REPLY")
                    {
                        msbBody = attach["replyInfo"]!["text"] + "\n" + attach["replyInfo"]!["replyName"]! + ":" + attach["replyInfo"]!["replyText"]!;
                        mcb.Plain(msbBody);
                    }
                    //礼物回复
                    else if (MsgType.Contains(messageType) && messageType == "GIFTREPLY")
                    {
                        msbBody = attach["giftReplyInfo"]!["text"] + "\n" + attach["giftReplyInfo"]!["replyName"]! + ":" + attach["giftReplyInfo"]!["replyText"]!;
                        mcb.Plain(msbBody);
                    }
                    //总选计分
                    //else if (false)
                    //{
                    //    msbBody = "送出了【" + attach["giftInfo"]!["giftName"] + "（" + attach["giftInfo"]!["tpNum"] + "分）】。";
                    //    mcb.Plain(msbBody);
                    //}
                    //直播
                    else if (MsgType.Contains(messageType) && messageType == "LIVEPUSH")
                    {
                        //判断是否at所有人
                        msbBody = "直播啦！\n标题：" + attach["livePushInfo"]!["liveTitle"];
                        mcb.Plain(msbBody).ImageFromUrl(Const.ConfigModel.KD.imgDomain + attach["livePushInfo"]!["liveCover"]!.ToString());
                        if (MsgType.FirstOrDefault(t => t == "AtAll")?.ToBool() ?? false)
                            mcb.AtAll();
                    }
                    //语音
                    else if (MsgType.Contains(messageType.ToLower()) && messageType == "AUDIO")
                    {
                        mcb.VoiceFromUrl(attach["audioInfo"]!["url"]!.ToString());
                    }
                    //视频
                    else if (MsgType.Contains(messageType.ToLower()) && messageType == "VIDEO")
                    {
                        mcb.VoiceFromUrl(attach["videoInfo"]!["url"]!.ToString());
                    }
                    // 房间电台
                    else if (MsgType.Contains(messageType) && messageType == "TEAM_VOICE")
                    {
                        //判断是否at所有人
                        msbBody = "开启了房间电台";
                        mcb.Plain(msbBody);
                        if (MsgType.FirstOrDefault(t => t == "AtAll")?.ToBool() ?? false)
                            mcb.AtAll();
                    }
                    //文字翻牌
                    else if (MsgType.Contains(messageType) && messageType == "FLIPCARD")
                    {
                        var answer = attach["filpCardInfo"]!["answer"]!.ToString();
                        mcb.Plain(answer.ToString());
                        mcb.Plain("\n粉丝提问：" + attach["filpCardInfo"]!["question"]);
                    }
                    //语音翻牌
                    else if (MsgType.Contains(messageType) && messageType == "FLIPCARD_AUDIO")
                    {
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.VoiceFromUrl(Const.ConfigModel.KD.mP4Domain + answer["url"]);
                        mcb.Plain("\n粉丝提问：" + attach["filpCardInfo"]!["question"]);
                    }
                    //视频翻牌
                    else if (MsgType.Contains(messageType) && messageType == "FLIPCARD_VIDEO")
                    {
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.Plain(Const.ConfigModel.KD.mP4Domain + answer["url"]);
                        mcb.Plain("\n粉丝提问：" + attach["filpCardInfo"]!["question"]);
                    }
                    //表情
                    else if (MsgType.Contains(messageType) && messageType == "EXPRESSIMAGE")
                    {
                        string url = attach["expressImgInfo"]!["emotionRemote"]!.ToString();
                        mcb.ImageFromUrl(url);
                    }
                    else return;
                }
                else return;
                if (!Const.MiraiConfig.useMirai) return;
                if (Const.ConfigModel.KD.forwardGroup)
                {
                    string group = !string.IsNullOrWhiteSpace(Const.ConfigModel.KD.group) ? Const.ConfigModel.KD.group : Const.ConfigModel.QQ.group;
                    List<string> groups = group.ToListV2();
                    MsgModel msgModel = new()
                    {
                        MsgChain = mcb.Build(),
                        Ids = groups,
                    };
                    msgModel.AddMsg();
                }
                if (Const.ConfigModel.KD.forwardQQ)
                {
                    MsgModel msgModel = new();
                    msgModel.Type = 2;
                    msgModel.MsgChain = mcb.Build();
                    if (!string.IsNullOrWhiteSpace(Const.ConfigModel.KD.qq))
                    {
                        var qqs = Const.ConfigModel.KD.qq.ToListV2();
                        msgModel.Ids = qqs;
                    }
                    else if (!string.IsNullOrWhiteSpace(Const.ConfigModel.QQ.admin))
                    {
                        msgModel.Id = Const.ConfigModel.QQ.admin;
                    }
                    msgModel.AddMsg();
                }
                return;
            }
            catch (Exception e)
            {
                e.AddLog();
                return;
            }
        }
        public static void LiveMsgReceiver(string str)
        {
            try
            {
                var result = JObject.Parse(str);
                var custom = JObject.Parse(result["custom"]?.ToString() ?? "");
                var roleId = custom["roleId"]?.ToInt();
                var messageType = result["type"]?.ToString();
                double timeVal = double.Parse(result["time"]?.ToString() ?? "0");
                var time = Convert.ToDateTime(DateTime.Parse(DateTime.Now.ToString("1970-01-01 08:00:00")).AddMilliseconds(timeVal).ToString());//返回为时间格式;
                if (roleId != 3) return;
                MessageChainBuilder mcb = new();
                mcb.Plain($"【{Const.ConfigModel.KD.name}|直播间】\n【{time}】\n{result["fromNick"]}:");
                if (messageType == "text")
                    mcb.Plain(result["text"]?.ToString());
                if (!Const.MiraiConfig.useMirai) return;
                if (Const.ConfigModel.KD.forwardGroup)
                {
                    string group = !string.IsNullOrWhiteSpace(Const.ConfigModel.KD.group) ? Const.ConfigModel.KD.group : Const.ConfigModel.QQ.group;
                    List<string> groups = group.ToListV2();
                    MsgModel msgModel = new()
                    {
                        MsgChain = mcb.Build(),
                        Ids = groups,
                    };
                    msgModel.AddMsg();
                }
                if (Const.ConfigModel.KD.forwardQQ)
                {
                    MsgModel msgModel = new();
                    msgModel.Type = 2;
                    msgModel.MsgChain = mcb.Build();
                    if (!string.IsNullOrWhiteSpace(Const.ConfigModel.KD.qq))
                    {
                        var qqs = Const.ConfigModel.KD.qq.ToListV2();
                        msgModel.Ids = qqs;
                    }
                    else if (!string.IsNullOrWhiteSpace(Const.ConfigModel.QQ.admin))
                    {
                        msgModel.Id = Const.ConfigModel.QQ.admin;
                    }
                    msgModel.AddMsg();
                }
                return;
            }
            catch (Exception e)
            {
                e.AddLog();
                return;
            }
        }

        private static void HandleEmoji(string str, ref MessageChainBuilder mcb)
        {
            string pattern = @"\[[^\]]+\]";
            MatchCollection matches = Regex.Matches(str, pattern);
            foreach (Match match in matches.Cast<Match>())
            {
                Console.WriteLine(match.Value);
            }
        }

        private static async Task<string> GetResponse(string url, string? data = null, string? token = null)
        {
            var baseUrl = "https://pocketapi.48.cn";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseUrl + url),
                Headers =
                    {
                        { "User-Agent", "PocketFans201807/6.0.16 (iPhone; iOS 13.5.1; Scale/2.00)" },
                        { "pa", "d6c1ae7a-5f06-4ef3-bb49-cb3e3c67e8fb" },
                        { "appInfo", "{\"vendor\":\"apple\",\"deviceId\":\"79DWUFH7-GWVM-HQH3-8DNG-ZPNPOTPEFGXW\",\"appVersion\":\"6.2.2\",\"appBuild\":\"21080401\",\"osVersion\":\"11.4.1\",\"osType\":\"ios\",\"deviceName\":\"iPhone XR\",\"os\":\"ios\"}" },
                        { "Accept-Language", "zh-Hans-AW;q=1" },
                        { "Host", "pocketapi.48.cn" },
                    },
            };
            if (token != null)
                request.Headers.Add("Token", token);
            if (data != null)
                request.Content = new StringContent(data)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                };
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }
        public static async Task<string> SmsCode(string mobile, string area = "86")
        {
            var data = new JObject()
            {
                {"mobile",mobile},
                {"area",area }
            };
            var res = await GetResponse("/user/api/v1/sms/send2", data.ToString());
            return res;
        }
        public static async Task<string> Login(string mobile, string smsCode)
        {
            var data = new JObject()
            {
                {"mobile",mobile},
                {"code",smsCode }
            };
            var res = await GetResponse("/user/api/v1/login/app/mobile/code", data.ToString());
            return res;
        }
        public static async Task<string> UserInfo(string token)
        {
            var res = await GetResponse("im/api/v1/im/userinfo", token: token);
            return res;
        }
    }
}