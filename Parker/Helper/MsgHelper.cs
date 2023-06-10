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

namespace Helper
{
    public static class Msg
    {
        public static MiraiBot _bot { get; set; }
        public static LiteContext? _liteContext { get; set; }
        #region 全局变量
        public static string Admin { get { return Const.ConfigModel.QQ.admin ?? ""; } }
        public static List<string> Check
        {
            get
            {
                _liteContext = new();
                return _liteContext.Caches.Where(t => t.type == 1).Select(t => t.content).ToList();
            }
        }
        public static List<string> Permission
        {
            get
            {
                return (Const.ConfigModel.QQ.permission ?? "").Split(",").ToList();
            }
        }
        public static List<string> Sensitive
        {
            get { return (Const.ConfigModel.QQ.sensitive ?? "").Split(",").ToList(); }
        }
        public static List<int> SensitiveAction
        {
            get { return (Const.ConfigModel.QQ.action ?? "").Split(",").Select(t => t.ToInt()).ToList(); }
        }
        public static List<string> FuncEnable
        {
            get { return (Const.ConfigModel.QQ.funcEnable ?? "").Split(",").ToList(); }
        }
        public static List<string> FuncAdmin
        {
            get { return (Const.ConfigModel.QQ.funcAdmin ?? "").Split(",").ToList(); }
        }
        public static List<string> FuncUser
        {
            get { return (Const.ConfigModel.QQ.funcUser ?? "").Split(",").ToList(); }
        }
        public static List<string> Group { get { return (Const.ConfigModel.QQ.group ?? "").Split(",").ToList(); } }
        public static List<RequestedEventBase> Event { get; set; } = new();
        #endregion

        public static void BotStart(MiraiBot bot)
        {
            _bot = bot;
            _liteContext = new();
            GroupMessageReceiver();
            FriendMessageReceiver();
            EventMessageReceiver();
        }

        public static void GroupMessageReceiver()
        {
            _bot.MessageReceived.OfType<GroupMessageReceiver>().Subscribe(async gmr =>
            {
                try
                {
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
                            await SendFriendMsg(Admin, $"群【{gmr.GroupName}】用户【{gmr.Sender.Name}】发送的“ {msgText} ”中含有敏感词！");
                        }
                        //撤回
                        if (SensitiveAction.Contains(5))
                        {
                            await gmr.RecallAsync();
                            var mcb = new MessageChainBuilder().At(gmr.Sender.Id).Plain(" 消息已被撤回，撤回原因：文字中包含敏感词！").Build();
                            await gmr.SendMessageAsync(mcb);
                        }
                    }
                }
                catch (Exception e)
                {
                    _liteContext = new();
                    await _liteContext.Logs.AddAsync(new()
                    {
                        message = e.Message,
                        createDate = DateTime.Now,
                    });
                    await _liteContext.SaveChangesAsync();
                    await _liteContext.DisposeAsync();
                    await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
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
                            "\n3、微博：\n#立即同步微博\n#同步微博#{链接地址}\n4、发送消息：\n#发送#{群/好友}#文字/图片/语音/视频/图文/{qq号/群号}/{文字/图片链接/{文字}-{图片链接}}" +
                            "\n5、功能开关：\n#开启/关闭模块{模块名称}\n#开启/关闭转发#{模块}#qq/群\n#修改转发#{模块}#qq/群#{值}";
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
                                var newMsgChain = new MessageChainBuilder().ImageFromBase64(Base64.UrlImgToBase64(item).Result).Build();
                                await fmr.SendMessageAsync(newMsgChain);
                                System.Threading.Thread.Sleep(1000);
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
                            if (index > Check.Count)
                            {
                                await fmr.SendMessageAsync($"未找到第{index}张图片，只有{Check.Count}张图片！");
                                return;
                            }
                            var newMsgChain = new MessageChainBuilder().ImageFromBase64(Base64.UrlImgToBase64(Check[index]).Result).Build();
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
                            if (index > Check.Count)
                            {
                                await fmr.SendMessageAsync($"未找到第{index}张图片，只有{Check.Count}张图片！");
                                return;
                            }
                            if (FileHelper.Save(Check[index]))
                            {
                                _liteContext = new();
                                var model = _liteContext.Caches.FirstOrDefault(t => t.content == Check[index] && t.type == 1)!;
                                _liteContext.Caches.Remove(model);
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
                            if (index > Check.Count)
                            {
                                await fmr.SendMessageAsync($"未找到第{index}张图片，只有{Check.Count}张图片！");
                                return;
                            }
                            _liteContext = new();
                            var model = _liteContext.Caches.FirstOrDefault(t => t.content == Check[index] && t.type == 1)!;
                            _liteContext.Caches.Remove(model);
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
                            var eventId = msgText.Replace("#同意#", "");
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
                                msgBuild.ImageFromBase64(Base64.UrlImgToBase64(list[3]).Result);
                            if (list[1] == "语音")
                                msgBuild.VoiceFromUrl(list[3]);
                            if (list[1] == "视频")
                                msgBuild.Plain(list[3]);
                            if (list[1] == "图文")
                                msgBuild.Plain(list[3].Split('-')[0]).ImageFromBase64(Base64.UrlImgToBase64(list[3].Split('-')[1]).Result);
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
                            Const.Enable[moudel] = "True";
                            Const._EnableModule = null;
                            await fmr.SendMessageAsync($"模块【{old}】已开启！");
                            _liteContext = new();
                            var model = _liteContext.Config.FirstOrDefault(t => t.parentId == 13 && t.key == moudel);
                            if (model != null)
                            {
                                model.value = "True";
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
                            Const.Enable[moudel] = "False";
                            Const._EnableModule = null;
                            await fmr.SendMessageAsync($"模块【{old}】已关闭！");
                            _liteContext = new();
                            var model = _liteContext.Config.FirstOrDefault(t => t.parentId == 13 && t.key == moudel);
                            if (model != null)
                            {
                                model.value = "False";
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
                            var pModel = _liteContext.Config.FirstOrDefault(t => t.parentId == 13 && t.key == moudel);
                            if (pModel != null)
                            {
                                var model = _liteContext.Config.FirstOrDefault(t => t.parentId == pModel.id && t.key == (type == "qq" ? "ForwardQQ" : "ForwardGroup"));
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
                            if (type == "qq")
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
                        if (msgText == "#最新日志")
                        {
                            _liteContext = new();
                            var log = _liteContext.Logs.OrderByDescending(t => t.createDate).FirstOrDefault();

                            await fmr.SendMessageAsync("时间：" + log?.createDate.ToString("yyyy-MM-dd HH:mm:ss") ?? "" + "\n详细：" + log?.message ?? "未查询到日志！");
                            return;
                        }
                    }
                    if (Permission.Contains(fmr.Sender.Id))
                    {

                    }
                }
                catch (Exception e)
                {
                    _liteContext = new();
                    await _liteContext.Logs.AddAsync(new()
                    {
                        message = e.Message,
                        createDate = DateTime.Now,
                    });
                    await _liteContext.SaveChangesAsync();
                    await _liteContext.DisposeAsync();
                    await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
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
                                await SendFriendMsg(Admin, $"机器人收到添加好友请求，事件标识：{qq!.EventId}，附加消息：{qq.Message}，请求人：{qq.FromId}，昵称：{qq.Nick}" + (string.IsNullOrWhiteSpace(qq.GroupId) ? "" : "，来自群：" + qq.GroupId));
                                Event.Add(qq);
                            };
                            break;
                        default:
                            break;
                        case Events.At:
                            {
                            };
                            break;
                        case Events.MemberJoined:
                            {
                            };
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _liteContext = new();
                    await _liteContext.Logs.AddAsync(new()
                    {
                        message = ex.Message,
                        createDate = DateTime.Now,
                    });
                    await _liteContext.SaveChangesAsync();
                    await _liteContext.DisposeAsync();
                    await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
                }
            });
        }
        private static string GetMoudel(string name)
        {
            var moudel = "";
            switch (name)
            {
                case "微博": moudel = "WB"; break;
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
                await group.SendGroupMessageAsync(msg);
            }
            catch (Exception ex)
            {
                _liteContext = new();
                await _liteContext.Logs.AddAsync(new()
                {
                    message = ex.Message,
                    createDate = DateTime.Now,
                });
                await _liteContext.SaveChangesAsync();
                await _liteContext.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
            }
        }

        public static async Task SendGroupMsg(string groupId, MessageChain msg)
        {
            try
            {
                if (_bot == null) return;
                var group = _bot.Groups.Value.FirstOrDefault(t => t.Id == groupId);
                await group.SendGroupMessageAsync(msg);
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
            }
            catch (Exception ex)
            {
                _liteContext = new();
                await _liteContext.Logs.AddAsync(new()
                {
                    message = ex.Message,
                    createDate = DateTime.Now,
                });
                await _liteContext.SaveChangesAsync();
                await _liteContext.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
            }
        }

        public static async Task SendFriendMsg(string friendId, string msg)
        {
            try
            {
                if (_bot == null) return;
                var friend = _bot.Friends.Value.FirstOrDefault(t => t.Id == friendId);
                await friend.SendFriendMessageAsync(msg);
            }
            catch (Exception ex)
            {
                _liteContext = new();
                await _liteContext.Logs.AddAsync(new()
                {
                    message = ex.Message,
                    createDate = DateTime.Now,
                });
                await _liteContext.SaveChangesAsync();
                await _liteContext.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
            }
        }
        public static async Task SendFriendMsg(string friendId, MessageChain msg)
        {
            try
            {

                if (_bot == null) return;
                var friend = _bot.Friends.Value.FirstOrDefault(t => t.Id == friendId);
                await friend.SendFriendMessageAsync(msg);
            }
            catch (Exception ex)
            {
                _liteContext = new();
                await _liteContext.Logs.AddAsync(new()
                {
                    message = ex.Message,
                    createDate = DateTime.Now,
                });
                await _liteContext.SaveChangesAsync();
                await _liteContext.DisposeAsync();
                await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
            }
        }
    }
}
