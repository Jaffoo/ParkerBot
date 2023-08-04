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
                //�Ƿ��ǼƷ�
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
                            //����������
                            var path = Directory.GetCurrentDirectory() + "/wwwroot/excel";
                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                            var excel = path + "/CelebrationScore" + DateTime.Now.ToString("MMdd") + ".csv";
                            if (!File.Exists(excel))
                            {
                                MiniExcel.SaveAs(excel, null);
                                MiniExcel.Insert(excel, new { �ڴ�ID = "�ڴ�ID", �ǳ� = "�ǳ�", ���� = "����", ʱ�� = "ʱ��", ��Դ = "��Դ" });
                            }
                            var value = new { �ڴ�ID = result["ext"]!["user"]!["userId"], �ǳ� = name, ���� = attachFen["giftInfo"]!["tpNum"]!, ʱ�� = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ��Դ = channelName };
                            MiniExcel.Insert(excel, value);
                        }
                    }
                }
                //if (!fen && roleId != 3) return;
                MessageChainBuilder mcb = new();
                mcb.Plain($"��{Const.ConfigModel.KD.name}|{channelName}��\n��{time}��\n{name}:");
                //ͼƬ
                if (MsgType.Contains(msgType) && msgType == "image")
                {
                    msbBody = result["attach"]!["url"]!.ToString();
                    mcb.ImageFromBase64(Base64.UrlImgToBase64(msbBody).Result);
                    await Task.Run(async () =>
                    {
                        await Weibo.FatchFace(msbBody);
                    });
                }
                //����
                else if (MsgType.Contains(msgType) && msgType == "text")
                {
                    //"230226137"
                    msbBody = result["body"]!.ToString();
                    mcb.Plain(msbBody);
                }
                //��Ƶ
                else if (MsgType.Contains(msgType) && msgType == "video")
                {
                    mcb.Plain(result["attach"]!["url"]!.ToString());
                }
                //����
                else if (MsgType.Contains(msgType) && msgType == "audio")
                {
                    mcb.VoiceFromUrl(result["attach"]!["url"]!.ToString());
                }
                else if (msgType == "custom")
                {
                    var attach = result["attach"]!;
                    var messageType = attach["messageType"]!.ToString();
                    //�ظ�
                    if (MsgType.Contains(messageType) && messageType == "REPLY")
                    {
                        msbBody = attach["replyInfo"]!["text"] + "\n" + attach["replyInfo"]!["replyName"]! + ":" + attach["replyInfo"]!["replyText"]!;
                        mcb.Plain(msbBody);
                    }
                    //����ظ�
                    else if (MsgType.Contains(messageType) && messageType == "GIFTREPLY")
                    {
                        msbBody = attach["giftReplyInfo"]!["text"] + "\n" + attach["giftReplyInfo"]!["replyName"]! + ":" + attach["giftReplyInfo"]!["replyText"]!;
                        mcb.Plain(msbBody);
                    }
                    //��ѡ�Ʒ�
                    else if (MsgType.Contains(messageType) && fen)
                    {
                        msbBody = "�ͳ��ˡ�" + attach["giftInfo"]!["giftName"] + "��" + attach["giftInfo"]!["tpNum"] + "�֣�����";
                        mcb.Plain(msbBody);
                    }
                    //ֱ��
                    else if (MsgType.Contains(messageType) && messageType == "LIVEPUSH")
                    {
                        //�ж��Ƿ�at������
                        msbBody = "ֱ������\n���⣺" + attach["livePushInfo"]!["liveTitle"];
                        mcb.Plain(msbBody).ImageFromBase64(Base64.UrlImgToBase64(Const.ConfigModel.KD.imgDomain + attach["livePushInfo"]!["liveCover"]!.ToString()).Result);
                        if (MsgType.FirstOrDefault(t => t == "AtAll")?.ToBool() ?? false)
                            mcb.AtAll();

                    }
                    //����
                    else if (MsgType.Contains(messageType.ToLower()) && messageType == "AUDIO")
                    {
                        mcb.VoiceFromUrl(attach["audioInfo"]!["url"]!.ToString());
                    }
                    //��Ƶ
                    else if (MsgType.Contains(messageType.ToLower()) && messageType == "VIDEO")
                    {
                        mcb.VoiceFromUrl(attach["videoInfo"]!["url"]!.ToString());
                    }
                    // �����̨
                    else if (MsgType.Contains(messageType) && messageType == "TEAM_VOICE")
                    {
                        //�ж��Ƿ�at������
                        msbBody = "�����˷����̨";
                        mcb.Plain(msbBody);
                        if (MsgType.FirstOrDefault(t => t == "AtAll")?.ToBool() ?? false)
                            mcb.AtAll();
                    }
                    //���ַ���
                    else if (MsgType.Contains(messageType) && messageType == "FLIPCARD")
                    {
                        await Msg.SendFriendMsg(Msg.Admin, "���ַ���:\n" + attach.ToString());
                        return;
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.Plain(answer.ToString());
                        mcb.Plain("���ַ��ƣ�" + attach["filpCardInfo"]!["question"]);
                    }
                    //��������
                    else if (MsgType.Contains(messageType) && messageType == "FLIPCARD_AUDIO")
                    {
                        await Msg.SendFriendMsg(Msg.Admin, "��������:\n" + attach.ToString());
                        return;
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.VoiceFromUrl(Const.ConfigModel.KD.mP4Domain + answer["url"]);
                        mcb.Plain("�������ƣ�" + attach["filpCardInfo"]!["question"]);
                    }
                    //��Ƶ����
                    else if (MsgType.Contains(messageType) && messageType == "FLIPCARD_VIDEO")
                    {
                        await Msg.SendFriendMsg(Msg.Admin, "��Ƶ����:\n" + attach.ToString());
                        return;
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.Plain(Const.ConfigModel.KD.mP4Domain + answer["url"]);
                        mcb.Plain("��Ƶ���ƣ�" + attach["filpCardInfo"]!["question"]);
                    }
                    //����
                    else if (MsgType.Contains(messageType) && messageType == "EXPRESSIMAGE")
                    {
                        string url = attach["expressImgInfo"]!["emotionRemote"]!.ToString();
                        mcb.ImageFromBase64(Base64.UrlImgToBase64(url).Result);
                    }
                    else if (MsgType.Count > 0)
                    {
                        await Msg.SendFriendMsg(Msg.Admin, "customδ֪��Ϣ\n" + attach.ToString());
                        return;
                    }
                    else return;
                }
                else if (MsgType.Count > 0)
                {
                    await Msg.SendFriendMsg(Msg.Admin, "δ֪��Ϣ����\n" + str);
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