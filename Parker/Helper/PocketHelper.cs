using Mirai.Net.Utils.Scaffolds;
using Newtonsoft.Json.Linq;
using ParkerBot;
using MiniExcelLibs;

namespace Helper
{
    public class Pocket
    {
        public static LiteContext? _liteContext { get; set; }
        private static readonly object _fileLock = new object();
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
                        var listFen = new List<string> { "0.1��", "1��", "9��", "99��", "999��" };
                        if (listFen.Contains(attachFen["giftInfo"]!["giftName"]!.ToString()))
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
                if (!fen && roleId != 3) return;
                if (roleId == 3)
                {
                    await Task.Run(() =>
                    {
                        lock (_fileLock)
                        {
                            var log = Directory.GetCurrentDirectory() + "/logs";
                            if (!Directory.Exists(log)) Directory.CreateDirectory(log);
                            var logFile = log + "/message" + DateTime.Now.ToString("MMdd") + ".txt";
                            if (!File.Exists(logFile)) File.Create(logFile);
                            using var sw = new StreamWriter(logFile, true);
                            sw.WriteLine(str);
                            sw.Dispose();
                            sw.Close();
                        }
                    });
                }
                MessageChainBuilder mcb = new();
                mcb.Plain($"��{channelName}��\n��{time}��\n{name}:");
                if (msgType == "image")
                {
                    msbBody = result["attach"]!["url"]!.ToString();
                    mcb.ImageFromBase64(Base64.UrlImgToBase64(msbBody).Result);
                    await Task.Run(async () =>
                    {
                        await Weibo.FatchFace(msbBody);
                    });
                }
                else if (msgType == "text")
                {
                    //"230226137"
                    msbBody = result["body"]!.ToString();
                    mcb.Plain(msbBody);
                }
                else if (msgType == "video")
                {
                    mcb.Plain(result["attach"]!["url"]!.ToString());
                }
                else if (msgType == "audio")
                {
                    mcb.VoiceFromUrl(result["attach"]!["url"]!.ToString());
                }
                else if (msgType == "custom")
                {
                    var attach = result["attach"]!;
                    if (attach["messageType"]!.ToString() == "REPLY")
                    {
                        msbBody = attach["replyInfo"]!["text"] + "\n" + attach["replyInfo"]!["replyName"]! + ":" + attach["replyInfo"]!["replyText"]!;
                        mcb.Plain(msbBody);
                    }
                    else if (attach["messageType"]!.ToString() == "GIFTREPLY")
                    {
                        msbBody = attach["giftReplyInfo"]!["text"] + "\n" + attach["giftReplyInfo"]!["replyName"]! + ":" + attach["giftReplyInfo"]!["replyText"]!;
                        mcb.Plain(msbBody);
                    }
                    else if (attach["messageType"]!.ToString() == "GIFT_TEXT")
                    {
                        msbBody = "Ϊ" + attach["giftInfo"]!["userName"]! + "����Ʒ�����" + attach["giftInfo"]!["giftName"] + "��";
                        mcb.Plain(msbBody);
                    }
                    else if (attach["messageType"]!.ToString() == "LIVEPUSH")
                    {
                        //�ж��Ƿ�at������
                        msbBody = "ֱ������\n���⣺" + attach["livePushInfo"]!["liveTitle"];
                        mcb.Plain(msbBody).ImageFromBase64(Base64.UrlImgToBase64(Const.ConfigModel.KD.imgDomain + attach["livePushInfo"]!["liveCover"]!.ToString()).Result);
                    }
                    else if (attach["messageType"]!.ToString() == "AUDIO")
                    {
                        mcb.VoiceFromUrl(attach["audioInfo"]!["url"]!.ToString());
                    }
                    else if (attach["messageType"]!.ToString() == "VIDEO")
                    {
                        mcb.VoiceFromUrl(attach["videoInfo"]!["url"]!.ToString());
                    }
                    // �����̨
                    else if (attach["messageType"]!.ToString() == "TEAM_VOICE")
                    {
                        //�ж��Ƿ�at������
                        msbBody = name + "�����˷����̨";
                        mcb.Plain(msbBody);
                    }
                    //���ַ���
                    else if (attach["messageType"]!.ToString() == "FLIPCARD")
                    {
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.Plain(answer.ToString());
                        mcb.Plain("���ַ��ƣ�" + attach["filpCardInfo"]!["question"]);
                        return;
                    }
                    //��������
                    else if (attach["messageType"]!.ToString() == "FLIPCARD_AUDIO")
                    {
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.VoiceFromUrl(Const.ConfigModel.KD.mP4Domain + answer["url"]);
                        mcb.Plain("�������ƣ�" + attach["filpCardInfo"]!["question"]);
                        return;
                    }
                    //��Ƶ����
                    else if (attach["messageType"]!.ToString() == "FLIPCARD_VIDEO")
                    {
                        var answer = JObject.Parse(attach["filpCardInfo"]!["answer"]!.ToString());
                        mcb.Plain(Const.ConfigModel.KD.mP4Domain + answer["url"]);
                        mcb.Plain("��Ƶ���ƣ�" + attach["filpCardInfo"]!["question"]);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    Console.WriteLine(msgType);
                    return;
                }
                if (Const.ConfigModel.KD.forwardGroup)
                {
                    string group = !string.IsNullOrWhiteSpace(Const.ConfigModel.KD.group) ? Const.ConfigModel.KD.group : Const.ConfigModel.QQ.group;
                    List<string> groups = !string.IsNullOrWhiteSpace(group) ? group.Split(",").ToList() : new();
                    groups.ForEach(async (item) =>
                    {
                        await Msg.SendGroupMsg(item, mcb.Build());
                    });
                }
                if (Const.ConfigModel.KD.forwardQQ)
                {
                    if (!string.IsNullOrWhiteSpace(Const.ConfigModel.KD.qq))
                    {
                        var qqs = Const.ConfigModel.KD.qq.Split(",").ToList();
                        qqs.ForEach(async (item) =>
                        {
                            await Msg.SendFriendMsg(item, mcb.Build());
                        });
                    }
                    else if (!string.IsNullOrWhiteSpace(Const.ConfigModel.QQ.admin))
                    {
                        await Msg.SendFriendMsg(Const.ConfigModel.QQ.admin, mcb.Build());
                    }
                }
            }
            catch (Exception e)
            {
                _liteContext = new();
                await _liteContext.Logs.AddAsync(new()
                {
                    message = "������Ϣ��\n" + e.Message + "\n��ջ��Ϣ��\n" + e.StackTrace,
                    createDate = DateTime.Now,
                });
                await _liteContext.SaveChangesAsync();
                await _liteContext.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "���򱨴��ˣ�����ϵ������������Ա��");
            }
        }
    }
}