using Mirai.Net.Utils.Scaffolds;
using Newtonsoft.Json.Linq;
using ParkerBot;
using MiniExcelLibs;
using System.Linq;

namespace Helper
{
    public class Pocket
    {
        public static LiteContext? _liteContext { get; set; }
        private static List<string> MsgType { get; set; } = Const.ConfigModel.KD.msgType.ToListV2();
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
                //是否是计分
                var fen = false;
                if (result.ContainsKey("attach"))
                {
                    if (result["fromAccount"]!.ToString() == "admin") return;
                    var attachFen = (JObject)result["attach"]!;
                    if (attachFen.ContainsKey("giftInfo"))
                    {
                        if (attachFen["giftInfo"]?["isScore"]?.ToString() == "1")
                        {
                            fen = true;
                            //创建工作表
                            var path = Directory.GetCurrentDirectory() + "/wwwroot/excel";
                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                            var excel = path + "/CelebrationScore" + DateTime.Now.ToString("MMdd") + ".csv";
                            if (!File.Exists(excel))
                            {
                                MiniExcel.SaveAs(excel, null);
                                MiniExcel.Insert(excel, new { 口袋ID = "口袋ID", 昵称 = "昵称", 分数 = "分数", 时间 = "时间", 来源 = "来源" });
                            }
                            var value = new { 口袋ID = result["ext"]!["user"]!["userId"], 昵称 = name, 分数 = attachFen["giftInfo"]!["tpNum"]!, 时间 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 来源 = channelName };
                            MiniExcel.Insert(excel, value);
                        }
                    }
                }
                //if (!fen && roleId != 3) return;
                MessageChainBuilder mcb = new();
                mcb.Plain($"【{Const.ConfigModel.KD.name}|{channelName}】\n【{time}】\n{name}:");
                //图片
                if (MsgType.Contains(msgType) && msgType == "image")
                {
                    msbBody = result["attach"]!["url"]!.ToString();
                    mcb.ImageFromBase64(Base64.UrlImgToBase64(msbBody).Result);
                    await Task.Run(async () =>
                    {
                        await Weibo.FatchFace(msbBody);
                    });
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
                    else if (MsgType.Contains(messageType) && fen)
                    {
                        msbBody = "送出了【" + attach["giftInfo"]!["giftName"] + "（" + attach["giftInfo"]!["tpNum"] + "分）】。";
                        mcb.Plain(msbBody);
                    }
                    //直播
                    else if (MsgType.Contains(messageType) && messageType == "LIVEPUSH")
                    {
                        //判断是否at所有人
                        msbBody = "直播啦！\n标题：" + attach["livePushInfo"]!["liveTitle"];
                        mcb.Plain(msbBody).ImageFromBase64(Base64.UrlImgToBase64(Const.ConfigModel.KD.imgDomain + attach["livePushInfo"]!["liveCover"]!.ToString()).Result);
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
                        await Msg.SendFriendMsg(Msg.Admin, "文字翻牌:\n" + attach.ToString());
                        return;
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.Plain(answer.ToString());
                        mcb.Plain("文字翻牌：" + attach["filpCardInfo"]!["question"]);
                    }
                    //语音翻牌
                    else if (MsgType.Contains(messageType) && messageType == "FLIPCARD_AUDIO")
                    {
                        await Msg.SendFriendMsg(Msg.Admin, "语音翻牌:\n" + attach.ToString());
                        return;
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.VoiceFromUrl(Const.ConfigModel.KD.mP4Domain + answer["url"]);
                        mcb.Plain("语音翻牌：" + attach["filpCardInfo"]!["question"]);
                    }
                    //视频翻牌
                    else if (MsgType.Contains(messageType) && messageType == "FLIPCARD_VIDEO")
                    {
                        await Msg.SendFriendMsg(Msg.Admin, "视频翻牌:\n" + attach.ToString());
                        return;
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.Plain(Const.ConfigModel.KD.mP4Domain + answer["url"]);
                        mcb.Plain("视频翻牌：" + attach["filpCardInfo"]!["question"]);
                    }
                    //表情
                    else if (MsgType.Contains(messageType) && messageType == "EXPRESSIMAGE")
                    {
                        string url = attach["expressImgInfo"]!["emotionRemote"]!.ToString();
                        mcb.ImageFromBase64(Base64.UrlImgToBase64(url).Result);
                    }
                    else if (MsgType.Count > 0)
                    {
                        await Msg.SendFriendMsg(Msg.Admin, "custom未知消息\n" + attach.ToString());
                        return;
                    }
                    else return;
                }
                else if (MsgType.Count > 0)
                {
                    await Msg.SendFriendMsg(Msg.Admin, "未知消息类型\n" + str);
                    return;
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
        public static async Task LiveMsgReceiver(string str)
        {
            _liteContext = new();
            Logs log = new()
            {
                message = str,
                createDate = DateTime.Now,
            };
            await _liteContext.Logs.AddAsync(log);
            await _liteContext.SaveChangesAsync();
            await _liteContext.DisposeAsync();
        }
    }
}