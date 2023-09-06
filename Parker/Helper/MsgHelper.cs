using Mirai.Net.Data.Messages.Concretes;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions;
using ParkerBot;
using Mirai.Net.Data.Events.Concretes.Request;
using Mirai.Net.Data.Events;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using Mirai.Net.Data.Shared;
using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Text;
using Mirai.Net.Data.Events.Concretes.Message;
using ParkerBot.Helper;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using Vanara.Extensions;
using FluentScheduler;
using static System.Net.Mime.MediaTypeNames;
using static Vanara.PInvoke.User32;

namespace Helper
{
    public class MsgModel
    {
        /// <summary>
        /// 1-群，2-好友
        /// </summary>
        public int Type { get; set; } = 1;
        public string Id { get; set; } = "";
        public List<string>? Ids { get; set; }
        public string? MsgStr { get; set; }
        public MessageChain? MsgChain { get; set; }
    }

    public static class Msg
    {
        public static Queue<MsgModel> MsgQueue = new();
        private static DateTime _lastSendTime = DateTime.Now;
        private static double _interval = 3;//单位秒
        private static MiraiBot _bot = new();
        public static LiteContext? _liteContext { get; set; }
        #region 全局变量
        public static string Admin => Const.ConfigModel.QQ.admin ?? "";
        public static bool Notice => Const.ConfigModel.QQ.notice;
        public static List<string> Check
        {
            get
            {
                _liteContext = new();
                return _liteContext.Caches.Where(t => t.type == 1).Select(t => t.content).ToList();
            }
        }
        public static List<string> Permission => Const.ConfigModel.QQ.permission.ToListV2();
        public static List<string> Sensitive => Const.ConfigModel.QQ.sensitive.ToListV2();
        public static List<int> SensitiveAction => Const.ConfigModel.QQ.action.ToListV2().Select(t => t.ToInt()).ToList();
        public static List<Config> FuncEnable => JsonConvert.DeserializeObject<List<Config>>(Const.ConfigModel.QQ.funcEnable) ?? new();
        public static List<string> FuncAdmin => Const.ConfigModel.QQ.funcAdmin.ToListV2();
        public static List<string> FuncUser => Const.ConfigModel.QQ.funcUser.ToListV2();
        public static List<string> Group => Const.ConfigModel.QQ.group.ToListV2();
        public static List<RequestedEventBase> Event { get; set; } = new();
        #endregion

        public static void BotStart(MiraiBot bot)
        {
            _bot = bot;
            _liteContext = new();
            GroupMessageReceiver();
            FriendMessageReceiver();
            EventMessageReceiver();
            Task.Run(HandlMsg);
        }

        public static void GroupMessageReceiver()
        {
            _bot.MessageReceived.OfType<GroupMessageReceiver>().Subscribe(async gmr =>
            {
                try
                {
                    if (!Group.Contains(gmr.GroupId)) return;
                    //消息链
                    var msgChain = gmr.MessageChain;
                    var msgText = gmr.MessageChain.GetPlainMessage();
                    _liteContext = new();
                    var msgModel = new ParkerBot.Message()
                    {
                        fromId = gmr.Sender.Id,
                        fromName = gmr.Sender.Name,
                        toId = gmr.GroupId,
                        toName = gmr.GroupName,
                        type = 2,
                        createDate = DateTime.Now,
                        msgType = 0,
                    };
                    foreach (var item in msgChain)
                    {
                        if (item.Type == Messages.Source)
                            msgModel.msgId = ((SourceMessage)item).MessageId;
                        if (item.Type == Messages.Plain)
                            msgModel.content = ((PlainMessage)item).Text;
                        if (item.Type == Messages.Image)
                            msgModel.img = ((ImageMessage)item).Url;
                        if (item.Type == Messages.Voice)
                            msgModel.voice = ((VoiceMessage)item).Url;
                        if (item.Type == Messages.FlashImage)
                            msgModel.other = ((FlashImageMessage)item).Url;
                    }
                    await _liteContext.Messages.AddAsync(msgModel);
                    await _liteContext.SaveChangesAsync();
                    if (Sensitive.Any(msgText.Contains))
                    {
                        //群内提示
                        if (SensitiveAction.Contains(1))
                        {
                            var mcb = new MessageChainBuilder().At(gmr.Sender.Id).Plain(" 您发送的消息文字中包含敏感词！").Build();
                            await gmr.SendMessageAsync(mcb);
                        }
                        //私信群管
                        if (SensitiveAction.Contains(2))
                        {
                            var qqAdmin = (await _bot.Groups.Value.FirstOrDefault(t => t.Id == gmr.GroupId).GetGroupMembersAsync()).Where(t => t.Permission == Permissions.Administrator || t.Permission == Permissions.Owner).Select(t => t.Id);
                            var friends = _bot.Friends.Value.Select(t => t.Id);
                            foreach (var item in qqAdmin)
                            {
                                if (friends.Any(t => t == item)) await SendFriendMsg(item, $"群【{gmr.GroupName}】用户【{gmr.Sender.Name}】发送的“{msgText}”中含有敏感词！");
                            }
                        }
                        //私信机器人管理员
                        if (SensitiveAction.Contains(3))
                        {
                            var friends = _bot.Friends.Value.Select(t => t.Id);
                            foreach (var item in Permission)
                            {
                                if (friends.Any(t => t == item)) await SendFriendMsg(item, $"群【{gmr.GroupName}】用户【{gmr.Sender.Name}】发送的“{msgText}”中含有敏感词！");
                            }
                        }
                        //私信超管
                        if (SensitiveAction.Contains(4))
                        {
                            await SendAdminMsg($"群【{gmr.GroupName}】用户【{gmr.Sender.Name}】发送的“ {msgText} ”中含有敏感词！");
                        }
                        //撤回
                        if (SensitiveAction.Contains(5))
                        {
                            await gmr.RecallAsync();
                            var mcb = new MessageChainBuilder().At(gmr.Sender.Id).Plain(" 消息已被撤回，撤回原因：文字中包含敏感词！").Build();
                            await gmr.SendMessageAsync(mcb);
                        }
                    }

                    #region 互动
                    //at消息
                    if (msgChain.Any(t => t.Type == Messages.At) && QQFunction.Keywrods.Contains(msgText.Trim()) && IsAuth("艾特作图", gmr.Sender.Id))
                    {
                        var model = msgChain.FirstOrDefault(t => t.Type == Messages.At);
                        if (model != null)
                        {
                            var atQQ = (model as AtMessage)!.Target;
                            var res = await QQFunction.AtPic(atQQ, msgText.Trim());
                            if (!string.IsNullOrWhiteSpace(res))
                            {
                                MessageChain builder = new MessageChainBuilder().ImageFromBase64(res).Build();
                                await gmr.SendMessageAsync(builder);
                                return;
                            }
                        }
                    }

                    if (msgText.Contains("问：") && IsAuth("问答", msgModel.fromId))
                    {
                        var result = await QQFunction.ChatGPT(msgText.Replace("问：", ""), msgModel.fromId);
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            result = result.Replace(@"\n", Environment.NewLine).Replace("\\\"", "\"");
                            await gmr.SendMessageAsync(result);
                            return;
                        }
                    }

                    if (msgText[0] == '?' && IsAuth("查缩写", msgModel.fromId))
                    {
                        var abbreviations = msgText[1..];
                        if (!string.IsNullOrWhiteSpace(abbreviations))
                        {
                            var msg = await QQFunction.Abbreviations(abbreviations);
                            if (!string.IsNullOrWhiteSpace(msg))
                            {
                                await gmr.SendMessageAsync(msg);
                                return;
                            }
                        }
                    }
                    #endregion
                    return;
                }
                catch (Exception e)
                {
                    e.AddLog();
                    return;
                }
            });
        }

        public static void FriendMessageReceiver()
        {
            _bot.MessageReceived.OfType<FriendMessageReceiver>().Subscribe(async fmr =>
            {
                //消息链
                var msgChain = fmr.MessageChain;
                var msgText = fmr.MessageChain.GetPlainMessage();
                try
                {
                    if (Admin == fmr.Sender.Id)
                    {
                        if (msgText == "#菜单")
                        {
                            string menu = "1、审核功能：\n#查看全部\n#查看#{第几张}\n#保存#{第几张}\n#保存全部\n#删除#{第几张}\n#删除全部\n2、好友申请：\n#同意/拒绝#{事件标识}" +
                            "\n3、微博：\n#立即同步微博\n#同步微博#{链接地址}" +
                            "\n4、发送消息：\n#发送#{群/好友}#文字/图片/语音/视频/图文/{qq号/群号}/{文字/图片链接/{文字}-{图片链接}}" +
                            "\n#微博用户搜索#{关键词}\n#关注微博用户#{用户id}\n#添加/删除微博关键词#{词}\n#重置微博关键词" +
                            "\n5、功能开关：\n#开启/关闭模块{模块名称}\n#开启/关闭转发#{模块}#qq/群\n#修改转发#{模块}#qq/群#{值}\n6、管理员：\n#添加/删除管理员#{qq}\n#删除全部管理员" +
                            "\n6、系统功能：\n#SQL#{sql}\n#清空聊天记录\n#修改#key/id#{key/id}#{value}";
                            await fmr.SendMessageAsync(menu);
                            return;
                        }
                        if (msgText == "#查看全部")
                        {
                            if (Check.Count == 0)
                            {
                                await fmr.SendMessageAsync("无待审核图片");
                                return;
                            }
                            foreach (var item in Check)
                            {
                                var newMsgChain = new MessageChainBuilder().ImageFromUrl(item).Build();
                                await fmr.SendMessageAsync(newMsgChain);
                                Thread.Sleep(1000);
                            }
                            return;
                        }
                        if (msgText.Contains("#查看#"))
                        {
                            if (Check.Count == 0)
                            {
                                await fmr.SendMessageAsync("无待审核图片");
                                return;
                            }
                            var index = msgText.Replace("#查看#", "").ToInt() - 1;
                            if (index >= Check.Count || index < 0)
                            {
                                await fmr.SendMessageAsync($"未找到图片");
                                return;
                            }
                            var newMsgChain = new MessageChainBuilder().ImageFromUrl(Check[index]).Build();
                            await fmr.SendMessageAsync(newMsgChain);
                            return;
                        }
                        if (msgText.Contains("#保存#"))
                        {
                            if (Check.Count == 0)
                            {
                                await fmr.SendMessageAsync("无待审核图片");
                                return;
                            }
                            var index = msgText.Replace("#保存#", "").ToInt() - 1;
                            if (index > Check.Count || index < 0)
                            {
                                await fmr.SendMessageAsync($"未找到张图片");
                                return;
                            }
                            if (FileHelper.Save(Check[index]))
                            {
                                _liteContext = new();
                                var model = await _liteContext.Caches.FirstOrDefaultAsync(t => t.content == Check[index] && t.type == 1)!;
                                if (model != null) _liteContext.Caches.Remove(model);
                                await _liteContext.SaveChangesAsync();
                            }
                            await fmr.SendMessageAsync("本地保存成功！");
                            return;
                        }
                        if (msgText == "#保存全部")
                        {
                            foreach (var item in Check)
                            {
                                _liteContext = new();
                                if (FileHelper.Save(item))
                                {
                                    var model = _liteContext.Caches.Where(t => t.type == 1)!;
                                    _liteContext.Caches.RemoveRange(model);
                                }
                                await _liteContext.SaveChangesAsync();
                            }
                            await fmr.SendMessageAsync("全部本地保存成功！");
                            return;
                        }
                        if (msgText.Contains("#删除#"))
                        {
                            if (Check.Count == 0)
                            {
                                await fmr.SendMessageAsync("无待审核图片");
                                return;
                            }
                            var index = msgText.Replace("#删除#", "").ToInt() - 1;
                            if (index > Check.Count || index < 0)
                            {
                                await fmr.SendMessageAsync($"未找到张图片");
                                return;
                            }
                            _liteContext = new();
                            var model = await _liteContext.Caches.FirstOrDefaultAsync(t => t.content == Check[index] && t.type == 1)!;
                            if (model != null) _liteContext.Caches.Remove(model);
                            await _liteContext.SaveChangesAsync();
                            await fmr.SendMessageAsync("删除成功！");
                            await Task.Run(Const.SetCache);
                            return;
                        }
                        if (msgText == "#删除全部")
                        {
                            _liteContext = new();
                            var list = _liteContext.Caches.Where(t => t.type == 1).ToList();
                            _liteContext.Caches.RemoveRange(list);
                            await _liteContext.SaveChangesAsync();
                            await fmr.SendMessageAsync("全部删除成功！");
                            await Task.Run(Const.SetCache);
                            return;
                        }

                        if (msgText.Contains("#同意#"))
                        {
                            var eventId = msgText.Replace("#同意#", "");
                            var friendEvent = Event.FirstOrDefault(t => t.EventId == eventId);
                            if (friendEvent == null)
                            {
                                await fmr.SendMessageAsync("未找到该好友申请！");
                                return;
                            }
                            await (friendEvent as NewFriendRequestedEvent).ApproveAsync();
                            await fmr.SendMessageAsync("已同意好友申请！");
                            return;
                        }
                        if (msgText.Contains("#拒绝#"))
                        {
                            var eventId = msgText.Replace("#拒绝#", "");
                            var friendEvent = Event.FirstOrDefault(t => t.EventId == eventId);
                            if (friendEvent == null)
                            {
                                await fmr.SendMessageAsync("未找到该好友申请！");
                                return;
                            }
                            await (friendEvent as NewFriendRequestedEvent).RejectAsync();
                            await fmr.SendMessageAsync("已拒绝好友申请！");
                            return;
                        }

                        if (msgText == "#立即同步微博")
                        {
                            await Weibo.Seve();
                            return;
                        }
                        if (msgText.Contains("#同步微博#"))
                        {
                            var url = msgText.Replace("#同步微博#", "");
                            await Weibo.SaveByUrl(url);
                            return;
                        }
                        if (msgText == "#最新日志")
                        {
                            _liteContext = new();
                            var log = await _liteContext.Logs.OrderByDescending(t => t.createDate).FirstOrDefaultAsync();
                            if (log == null) return;
                            if (log.message.Length > 500) log.message = log.message.Substring(0, 100);
                            MessageChain mc = new()
                            {
                                new PlainMessage("时间：" + log?.createDate.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
                                new PlainMessage("\n详细：" + log?.message ?? "未查询到日志！")
                            };
                            await fmr.SendMessageAsync(mc);
                            return;
                        }
                        if (msgText.Contains("#添加管理员#"))
                        {
                            var qqNum = msgText.Replace("#添加管理员#", "");
                            if (string.IsNullOrWhiteSpace(qqNum.Trim()))
                            {
                                await fmr.SendMessageAsync("格式错误!");
                                return;
                            }
                            if (Permission.Contains(qqNum))
                                await fmr.SendMessageAsync("此账号已是管理员！");
                            _liteContext = new();
                            var model = await _liteContext.Config.FirstOrDefaultAsync(t => t.key == "Permission");
                            if (model == null) throw new Exception("key为【Permission】的值不存在！");
                            Const.Config["QQ"]!["Permission"] = Const.Config["QQ"]!["Permission"]!.ToString() + "," + qqNum;
                            Const._ConfigModel = null;
                            model.value = Const.Config["QQ"]!["Permission"]!.ToString();
                            _liteContext.Update(model);
                            await _liteContext.SaveChangesAsync();
                            await _liteContext.DisposeAsync();
                            await fmr.SendMessageAsync("添加成功！");
                            return;
                        }
                        if (msgText == "#删除全部管理员")
                        {
                            _liteContext = new();
                            var model = await _liteContext.Config.FirstOrDefaultAsync(t => t.key == "Permission");
                            if (model == null) throw new Exception("key为【Permission】的值不存在！");
                            model.value = "";
                            _liteContext.Update(model);
                            await _liteContext.SaveChangesAsync();
                            await _liteContext.DisposeAsync();
                            Const.Config["QQ"]!["Permission"] = model.value;
                            Const._ConfigModel = null;
                            await fmr.SendMessageAsync("删除成功！");
                            return;
                        }
                        if (msgText.Contains("#删除管理员#"))
                        {
                            var qqNum = msgText.Replace("#删除管理员#", "");
                            if (string.IsNullOrWhiteSpace(qqNum.Trim()))
                            {
                                await fmr.SendMessageAsync("格式错误!");
                                return;
                            }
                            if (!Permission.Contains(qqNum))
                            {
                                await fmr.SendMessageAsync("此账号不是管理员！");
                                return;
                            }
                            Permission.Remove(qqNum);
                            _liteContext = new();
                            var model = await _liteContext.Config.FirstOrDefaultAsync(t => t.key == "Permission");
                            if (model == null) throw new Exception("key为【Permission】的值不存在！");
                            model.value = string.Join(",", Permission);
                            _liteContext.Update(model);
                            await _liteContext.SaveChangesAsync();
                            await _liteContext.DisposeAsync();
                            Const.Config["QQ"]!["Permission"] = model.value;
                            Const._ConfigModel = null;
                            await fmr.SendMessageAsync("删除成功！");
                            return;
                        }
                        if (msgText.Contains("#SQL#"))
                        {
                            var sql = msgText.Replace("#SQL#", "");
                            if (string.IsNullOrWhiteSpace(sql))
                            {
                                await fmr.SendMessageAsync("sql语句为空");
                                return;
                            }
                            _liteContext = new();
                            var res = await _liteContext.Database.ExecuteSqlRawAsync(sql);
                            await fmr.SendMessageAsync("执行成功，受影响行数：" + res + "行；");
                            return;
                        }
                        if (msgText.Contains("#清空聊天记录"))
                        {
                            _liteContext = new();
                            var sql = "delete from message;";
                            var res = await _liteContext.Database.ExecuteSqlRawAsync(sql);
                            await fmr.SendMessageAsync("清空成功");
                            return;
                        }
                        if (msgText.Contains("#修改#key#"))
                        {
                            var words = msgText.Replace("#修改#key#", "");
                            if (string.IsNullOrWhiteSpace(words))
                            {
                                await fmr.SendMessageAsync("格式错误！");
                                return;
                            }
                            var list = words.Split('#');
                            if (list.Length != 2)
                            {
                                await fmr.SendMessageAsync("格式错误！");
                                return;
                            }
                            var key = list[0];
                            var value = list[1];
                            _liteContext = new();
                            var model = _liteContext.Config.FirstOrDefault(t => t.key == key);
                            if (model == null)
                            {
                                await fmr.SendMessageAsync("key不存在！");
                                return;
                            }
                            var old = model.value;
                            model.value = value;
                            _liteContext.Update(model);
                            _liteContext.SaveChanges();
                            await fmr.SendMessageAsync($"【{key}】值已修改。{old} -> {value}");
                            return;
                        }
                        if (msgText.Contains("#修改#id#"))
                        {
                            var words = msgText.Replace("#修改#id#", "");
                            if (string.IsNullOrWhiteSpace(words))
                            {
                                await fmr.SendMessageAsync("格式错误！");
                                return;
                            }
                            var list = words.Split('#');
                            if (list.Length != 2)
                            {
                                await fmr.SendMessageAsync("格式错误！");
                                return;
                            }
                            var id = list[0];
                            var value = list[1];
                            _liteContext = new();
                            var model = _liteContext.Config.FirstOrDefault(t => t.id == id.ToInt());
                            if (model == null)
                            {
                                await fmr.SendMessageAsync("数据不存在！");
                                return;
                            }
                            var old = model.value;
                            model.value = value;
                            _liteContext.Update(model);
                            _liteContext.SaveChanges();
                            await fmr.SendMessageAsync($"id:{id}【{model.key}】值已修改。{old} -> {value}");
                            return;
                        }
                    }
                    if (Permission.Contains(fmr.Sender.Id) || fmr.Sender.Id == Admin)
                    {
                        if (msgText == "#菜单" && Permission.Contains(fmr.Sender.Id))
                        {
                            string menu = "1、功能开关：\n#开启/关闭模块{模块名称}\n#开启/关闭转发#{模块}#qq/群\n#修改转发#{模块}#qq/群#{值}\n2、发送消息：#发送#{群/好友}#文字/图片/语音/视频/图文/{qq号/群号}/{文字/图片链接/{文字}-{图片链接}}" +
                            "\n3、微博：\n#微博用户搜索#{关键词}\n#关注微博用户#{用户id}\n#添加/删除微博关键词#{词}\n#重置微博关键词";
                            await fmr.SendMessageAsync(menu);
                            return;
                        }
                        if (msgText.Contains("#发送#"))
                        {
                            var msg = msgText.Replace("#发送#", "");
                            var list = msg.Split('#').ToList();
                            if (list.Count != 4)
                            {
                                await fmr.SendMessageAsync("格式错误！");
                                return;
                            }
                            var msgBuild = new MessageChainBuilder();
                            if (list[1] == "文字")
                                msgBuild.Plain(list[3]);
                            if (list[1] == "图片")
                                msgBuild.ImageFromUrl(list[3]);
                            if (list[1] == "语音")
                                msgBuild.VoiceFromUrl(list[3]);
                            if (list[1] == "视频")
                                msgBuild.Plain(list[3]);
                            if (list[1] == "图文")
                                msgBuild.Plain(list[3].Split('-')[0]).ImageFromUrl(list[3].Split('-')[1]);
                            if (list[0] == "好友")
                                await SendFriendMsg(list[2], msgBuild.Build());
                            if (list[0] == "群")
                                await SendGroupMsg(list[2], msgBuild.Build());
                            return;
                        }
                        if (msgText.Contains("#开启模块#"))
                        {
                            var moudel = msgText.Replace("#开启模块#", "");
                            var old = moudel;
                            moudel = GetMoudel(moudel);
                            Const.Enable[moudel] = "true";
                            Const._EnableModule = null;
                            await fmr.SendMessageAsync($"模块【{old}】已开启！");
                            JobManager.RemoveAllJobs();
                            JobManager.Initialize(new FluentSchedulerFactory());
                            _liteContext = new();
                            var model = await _liteContext.Config.FirstOrDefaultAsync(t => t.parentId == 13 && t.key == moudel);
                            if (model != null)
                            {
                                model.value = "true";
                                _liteContext.Update(model);
                                await _liteContext.SaveChangesAsync();
                            }
                            await _liteContext.DisposeAsync();
                            return;
                        }
                        if (msgText.Contains("#关闭模块#"))
                        {
                            var moudel = msgText.Replace("#关闭模块#", "");
                            var old = moudel;
                            moudel = GetMoudel(moudel);
                            Const.Enable[moudel] = "false";
                            Const._EnableModule = null;
                            await fmr.SendMessageAsync($"模块【{old}】已关闭！");
                            JobManager.RemoveJob(moudel);
                            _liteContext = new();
                            var model = await _liteContext.Config.FirstOrDefaultAsync(t => t.parentId == 13 && t.key == moudel);
                            if (model != null)
                            {
                                model.value = "false";
                                _liteContext.Update(model);
                                await _liteContext.SaveChangesAsync();
                            }
                            await _liteContext.DisposeAsync();
                            return;
                        }
                        if (msgText.Contains("#开启转发#"))
                        {
                            var list = msgText.Replace("#开启转发#", "").Split('#').ToList();
                            if (list.Count != 2)
                            {
                                await fmr.SendMessageAsync("格式错误！");
                                return;
                            }
                            var moudel = list[0];
                            var type = list[1];
                            moudel = GetMoudel(moudel);
                            if (!Const.Config.ContainsKey(moudel))
                            {
                                await fmr.SendMessageAsync("模块不存在！");
                                return;
                            }
                            if (string.IsNullOrWhiteSpace(type))
                                type = "群";
                            if (type == "qq")
                                Const.Config[moudel]!["ForwardQQ"] = "True";
                            if (type == "群")
                                Const.Config[moudel]!["ForwardGroup"] = "True";
                            Const._ConfigModel = null;
                            await fmr.SendMessageAsync($"模块【{list[0]}】转发至{type}功能已开启！");
                            _liteContext = new();
                            var pList = _liteContext.Config.Where(t => t.parentId == 13).Select(t => t.id).ToList();
                            var cList = _liteContext.Config.Where(t => pList.Contains(t.id)).ToList();
                            var model = cList.FirstOrDefault(t => t.key == (type == "qq" ? "ForwardQQ" : "ForwardGroup"));
                            if (model != null)
                            {
                                model.value = "True";
                                _liteContext.Update(model);
                                await _liteContext.SaveChangesAsync();
                            }
                            await _liteContext.DisposeAsync();
                            return;
                        }
                        if (msgText.Contains("#关闭转发#"))
                        {
                            var list = msgText.Replace("#关闭转发#", "").Split('#').ToList();
                            if (list.Count != 2)
                            {
                                await fmr.SendMessageAsync("格式错误！");
                                return;
                            }
                            var moudel = list[0];
                            var type = list[1];
                            moudel = GetMoudel(moudel);
                            if (!Const.Config.ContainsKey(moudel))
                            {
                                await fmr.SendMessageAsync("模块不存在！");
                                return;
                            }
                            if (string.IsNullOrWhiteSpace(type))
                                type = "群";
                            if (type == "qq")
                                Const.Config[moudel]!["ForwardQQ"] = "False";
                            if (type == "群")
                                Const.Config[moudel]!["ForwardGroup"] = "False";
                            Const._ConfigModel = null;
                            await fmr.SendMessageAsync($"模块【{list[0]}】转发至{type}功能已关闭！");
                            _liteContext = new();
                            var pModel = await _liteContext.Config.FirstOrDefaultAsync(t => t.parentId == 13 && t.key == moudel);
                            if (pModel != null)
                            {
                                var model = await _liteContext.Config.FirstOrDefaultAsync(t => t.parentId == pModel.id && t.key == (type == "qq" ? "ForwardQQ" : "ForwardGroup"));
                                if (model != null)
                                {
                                    model.value = "False";
                                    _liteContext.Update(model);
                                    await _liteContext.SaveChangesAsync();
                                }
                                await _liteContext.DisposeAsync();
                            }
                            return;
                        }
                        if (msgText.Contains("#修改转发#"))
                        {
                            var list = msgText.Replace("#修改转发#", "").Split('#').ToList();
                            if (list.Count != 3)
                            {
                                await fmr.SendMessageAsync("格式错误！");
                                return;
                            }
                            var moudel = list[0];
                            var type = list[1];
                            var value = list[2];
                            moudel = GetMoudel(moudel);
                            if (!Const.Config.ContainsKey(moudel))
                            {
                                await fmr.SendMessageAsync("模块不存在！");
                                return;
                            }
                            if (string.IsNullOrWhiteSpace(type))
                                type = "群";
                            if (type == "qq" || type == "QQ")
                                Const.Config[moudel]!["QQ"] = value;
                            if (type == "群")
                                Const.Config[moudel]!["Group"] = value;
                            Const._ConfigModel = null;
                            await fmr.SendMessageAsync($"模块【{list[0]}】转发至{type}功能的值已修改为：{value}");
                            _liteContext = new();
                            var pList = _liteContext.Config.Where(t => t.parentId == 13).Select(t => t.id).ToList();
                            var cList = _liteContext.Config.Where(t => pList.Contains(t.id)).ToList();
                            var model = cList.FirstOrDefault(t => t.key == (type == "qq" ? "QQ" : "Group"));
                            if (model != null)
                            {
                                model.value = value;
                                _liteContext.Update(model);
                                await _liteContext.SaveChangesAsync();
                            }
                            await _liteContext.DisposeAsync();
                            return;
                        }
                        if (msgText.Contains("#微博用户搜索#"))
                        {
                            var keyword = msgText.Replace("#微博用户搜索#", "");
                            var url = $"https://m.weibo.cn/api/container/getIndex?containerid=100103type%3D3%26q%3D{keyword}%26t%3D%26page_type%3Dsearchall";
                            var handler = new HttpClientHandler() { UseCookies = true };
                            HttpClient httpClient = new(handler);
                            httpClient.DefaultRequestHeaders.Add("user-agent", @"Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Mobile Safari/537.36 Edg/114.0.1823");
                            httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not.A/Brand\"; v = \"8\", \"Chromium\"; v = \"114\", \"Microsoft Edge\"; v = \"114\"");
                            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "Windows");
                            httpClient.DefaultRequestHeaders.Add("cookie", "_T_WM=98249609066; WEIBOCN_FROM=1110005030; MLOGIN=0; XSRF-TOKEN=4c5095; mweibo_short_token=1937e050d5; M_WEIBOCN_PARAMS=luicode%3D10000011%26lfid%3D100103type%253D3%2526q%253D%25E9%25BB%2584%25E6%2580%25A1%25E6%2585%2588%2526t%253D%26fid%3D100103type%253D3%2526q%253D%25E9%25BB%2584%25E6%2580%25A1%25E6%2585%2588%2526t%253D%26uicode%3D10000011");
                            var res = await httpClient.GetAsync(url);
                            var content = await res.Content.ReadAsStringAsync();
                            var result = JObject.Parse(content);
                            var resultList = JArray.FromObject(result["data"]!["cards"]!);
                            var model = resultList.FirstOrDefault(t => t["card_type"]?.ToString() == "11");
                            if (model == null) return;
                            var list = JArray.FromObject(model["card_group"]!);
                            StringBuilder msg = new("以为你搜索到以下结果：\n");
                            for (int i = 0; i < list.Count; i++)
                            {
                                var user = list[i];
                                msg.Append($"{i + 1}：{user!["user"]!["screen_name"]}({user!["user"]!["id"]}),{user!["desc1"]},{user!["desc2"]}\n");
                                if (i == 2) break;
                            }
                            msg.Append("注：结果有多个时，仅展示前三个！");
                            await fmr.SendMessageAsync(msg.ToString());
                            return;
                        }
                        if (msgText.Contains("#删除微博关键词#"))
                        {
                            var keywords = msgText.Replace("#删除微博关键词#", "");
                            if (string.IsNullOrWhiteSpace(keywords))
                                await fmr.SendMessageAsync("输入内容为空！");
                            if (!Weibo.Keywords.Contains(keywords))
                                await fmr.SendMessageAsync("不存在该关键词！");
                            Weibo.Keywords.Remove(keywords);
                            _liteContext = new();
                            var model = _liteContext.Config.FirstOrDefault(t => t.id == 87);
                            model!.value = string.Join(",", Weibo.Keywords);
                            _liteContext.Update(model);
                            _liteContext.SaveChanges();
                            Const._ConfigModel = null;
                            await fmr.SendMessageAsync($"已删除关键词【{keywords}】");
                            return;
                        }
                        if (msgText.Contains("#添加微博关键词#"))
                        {
                            var keywords = msgText.Replace("#添加微博关键词#", "");
                            if (string.IsNullOrWhiteSpace(keywords))
                                await fmr.SendMessageAsync("输入内容为空！");
                            if (Weibo.Keywords.Contains(keywords))
                                await fmr.SendMessageAsync("已存在该关键词！");
                            Weibo.Keywords.Add(keywords);
                            _liteContext = new();
                            var model = _liteContext.Config.FirstOrDefault(t => t.id == 87);
                            model!.value = string.Join(",", Weibo.Keywords);
                            _liteContext.Update(model);
                            _liteContext.SaveChanges();
                            Const._ConfigModel = null;
                            await fmr.SendMessageAsync($"已添加关键词【{keywords}】");
                            return;
                        }
                        if (msgText.Contains("#重置微博关键词"))
                        {
                            _liteContext = new();
                            var model = _liteContext.Config.FirstOrDefault(t => t.id == 87);
                            model!.value = "";
                            _liteContext.Update(model);
                            _liteContext.SaveChanges();
                            Const._ConfigModel = null;
                            await fmr.SendMessageAsync($"已重置关键词");
                            return;
                        }
                    }
                    if (msgText.Contains("问："))
                    {
                        var result = await QQFunction.ChatGPT(msgText.Replace("问：", ""), fmr.Sender.Id);
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            result = result.Replace(@"\n", Environment.NewLine).Replace("\\\"", "\"");
                            await fmr.SendMessageAsync(result);
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    e.AddLog();
                    return;
                }
            });
        }

        public static void EventMessageReceiver()
        {
            _bot.EventReceived.OfType<EventBase>().Subscribe(async e =>
            {
                try
                {
                    switch (e.Type)
                    {
                        case Events.NewFriendRequested:
                            {
                                var qq = e as NewFriendRequestedEvent;
                                if (qq == null) return;
                                await SendFriendMsg(Admin, $"机器人收到添加好友请求\n事件标识：{qq.EventId}\n附加消息：{qq.Message}\n请求人：{qq.FromId}\n昵称：{qq.Nick}" + (string.IsNullOrWhiteSpace(qq.GroupId) ? "" : "\n来自群：" + qq.GroupId));
                                Event.Add(qq);
                            };
                            return;
                        case Events.At:
                            {
                                var qq = e as AtEvent;
                                if (qq == null) return;
                                var text = qq.Receiver.MessageChain.GetPlainMessage().Trim();
                                var sender = qq.Receiver.Sender.Id;
                                if (text.Substring(text.Length - 2, 2) == "文案" && IsAuth("文案", sender))
                                {
                                    var keyword = text.Replace("文案", "");
                                    var res = await QQFunction.WenAn(keyword);
                                    await qq.Receiver.SendMessageAsync(res);
                                    return;
                                }
                                if (text.Substring(text.Length - 2, 2) == "天气" && IsAuth("天气", sender))
                                {
                                    var keyword = text.Replace("天气", "");
                                    var res = await QQFunction.Weather(keyword);
                                    await qq.Receiver.SendMessageAsync(res);
                                    return;
                                }
                                if (text == "舔狗文学" && IsAuth("舔狗", sender))
                                {
                                    var res = await QQFunction.Dog();
                                    await qq.Receiver.SendMessageAsync(res);
                                    return;
                                }
                                if (IsAuth("问答", sender))
                                {
                                    var result = await QQFunction.XiaoAi(text);
                                    if (string.IsNullOrWhiteSpace(result)) return;
                                    result = result.Replace(@"\n", Environment.NewLine).Replace("\\\"", "\"");
                                    await qq.Receiver.SendMessageAsync(result);
                                }
                            };
                            return;
                        case Events.MemberJoined:
                            {
                            };
                            return;
                        default:
                            return;
                    }
                }
                catch (Exception ex)
                {
                    ex.AddLog();
                    return;
                }
            });
        }
        private static bool IsAuth(string keyword, string qq)
        {
            if (!FuncEnable.Any(t => t.value == keyword)) return false;
            if (!Permission.Contains(qq) && qq != Admin)
                if (!FuncUser.Any(t => t == keyword))
                    return false;
            return true;
        }
        private static string GetMoudel(string name)
        {
            var moudel = "";
            switch (name)
            {
                case "微博": moudel = "WB"; break;
                case "微博吃瓜": moudel = "WBChiGua"; break;
                case "口袋": moudel = "KD"; break;
                case "口袋48": moudel = "KD"; break;
                case "B站": moudel = "BZ"; break;
                case "抖音": moudel = "DY"; break;
                case "小红书": moudel = "XHS"; break;
                default:
                    break;
            }
            return moudel;
        }

        public static async Task SendGroupMsg(string groupId, string msg)
        {
            try
            {
                if (_bot == null) return;
                var group = _bot.Groups.Value.FirstOrDefault(t => t.Id == groupId);
                if (group == null) return;
                await group.SendGroupMessageAsync(msg);
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
            }
        }

        public static async Task SendGroupMsg(List<string> groupIds, string msg)
        {
            try
            {
                if (_bot == null) return;
                foreach (var groupId in groupIds)
                {
                    var group = _bot.Groups.Value.FirstOrDefault(t => t.Id == groupId);
                    if (group == null) continue;
                    await group.SendGroupMessageAsync(msg);
                }
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }

        public static async Task SendGroupMsg(string groupId, MessageChain msg)
        {
            try
            {
                if (_bot == null) return;
                var group = _bot.Groups.Value.FirstOrDefault(t => t.Id == groupId);
                if (group == null) return;
                await group.SendGroupMessageAsync(msg);
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }

        public static async Task SendGroupMsg(List<string> groupIds, MessageChain msg)
        {
            try
            {
                if (_bot == null) return;
                foreach (var groupId in groupIds)
                {
                    var group = _bot.Groups.Value.FirstOrDefault(t => t.Id == groupId);
                    if (group == null) continue;
                    await group.SendGroupMessageAsync(msg);
                }
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }

        public static async Task SendFriendMsg(string friendId, string msg)
        {
            try
            {
                if (_bot == null) return;
                var friend = _bot.Friends.Value.FirstOrDefault(t => t.Id == friendId);
                if (friend == null) return;
                await friend.SendFriendMessageAsync(msg);
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }

        public static async Task SendFriendMsg(List<string> friendIds, string msg)
        {
            try
            {
                if (_bot == null) return;
                foreach (var friendId in friendIds)
                {
                    var friend = _bot.Friends.Value.FirstOrDefault(t => t.Id == friendId);
                    if (friend == null) continue;
                    await friend.SendFriendMessageAsync(msg);
                }
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }

        public static async Task SendFriendMsg(string friendId, MessageChain msg)
        {
            try
            {
                if (_bot == null) return;
                var friend = _bot.Friends.Value.FirstOrDefault(t => t.Id == friendId);
                if (friend == null) return;
                await friend.SendFriendMessageAsync(msg);
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }

        public static async Task SendFriendMsg(List<string> friendIds, MessageChain msg)
        {
            try
            {
                if (_bot == null) return;
                foreach (var friendId in friendIds)
                {
                    var friend = _bot.Friends.Value.FirstOrDefault(t => t.Id == friendId);
                    if (friend == null) continue;
                    await friend.SendFriendMessageAsync(msg);
                }
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }

        public static async Task SendAdminMsg(string msg)
        {
            try
            {
                if (_bot == null || !Notice) return;
                var friend = _bot.Friends.Value.FirstOrDefault(t => t.Id == Admin);
                if (friend == null) return;
                await friend.SendFriendMessageAsync(msg);
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }
        public static async Task SendAdminMsg(MessageChain msg)
        {
            try
            {
                if (_bot == null || !Notice) return;
                var friend = _bot.Friends.Value.FirstOrDefault(t => t.Id == Admin);
                if (friend == null) return;
                await friend.SendFriendMessageAsync(msg);
                return;
            }
            catch (Exception ex)
            {
                ex.AddLog();
                return;
            }
        }

        public static async void HandlMsg()
        {
            while (true)
            {
                try
                {
                    double interval = (DateTime.Now - _lastSendTime).TotalSeconds;
                    if (MsgQueue.Count > 0 && interval >= _interval)
                    {
                        var msgModel = MsgQueue.Dequeue();
                        if (msgModel.Type == 1)
                        {
                            if (msgModel.Ids != null)
                            {
                                if (msgModel.MsgChain?.Count > 0)
                                {
                                    await SendGroupMsg(msgModel.Ids, msgModel.MsgChain);
                                }
                                else if (!string.IsNullOrWhiteSpace(msgModel.MsgStr)) await SendGroupMsg(msgModel.Ids, msgModel.MsgStr);
                            }
                            else if (!string.IsNullOrWhiteSpace(msgModel.Id))
                            {
                                if (msgModel.MsgChain?.Count > 0) await SendGroupMsg(msgModel.Id, msgModel.MsgChain);
                                else if (!string.IsNullOrWhiteSpace(msgModel.MsgStr)) await SendGroupMsg(msgModel.Id, msgModel.MsgStr);
                            }
                        }
                        if (msgModel.Type == 2)
                        {
                            if (msgModel.Ids != null)
                            {
                                if (msgModel.MsgChain?.Count > 0) await SendFriendMsg(msgModel.Ids, msgModel.MsgChain);
                                else if (!string.IsNullOrWhiteSpace(msgModel.MsgStr)) await SendFriendMsg(msgModel.Ids, msgModel.MsgStr);
                            }
                            else if (!string.IsNullOrWhiteSpace(msgModel.Id))
                            {
                                if (msgModel.MsgChain?.Count > 0) await SendFriendMsg(msgModel.Id, msgModel.MsgChain);
                                else if (!string.IsNullOrWhiteSpace(msgModel.MsgStr)) await SendFriendMsg(msgModel.Id, msgModel.MsgStr);
                            }
                        }
                        _lastSendTime = DateTime.Now;
                    }
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    e.AddLog();
                    continue;
                }
            }
        }

        public static void AddMsg(this MsgModel model)
        {
            MsgQueue.Enqueue(model);
        }
    }
}
