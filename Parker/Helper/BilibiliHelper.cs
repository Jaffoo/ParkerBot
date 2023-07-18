using Mirai.Net.Utils.Scaffolds;
using Newtonsoft.Json.Linq;
using ParkerBot;

namespace Helper
{
    public class Bilibili
    {
        public static LiteContext? dbContext;
        public static List<string> Uids
        {
            get { return Const.ConfigModel.BZ.url.ToListV2(); }
        }

        public static int TimeSpan
        {
            get { return Const.ConfigModel.BZ.timeSpan.ToInt(); }
        }

        public static async Task Monitor()
        {
            try
            {
                var index = -1;
                foreach (var item in Uids)
                {
                    index++;
                    var url = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?host_uid=" + item;
                    HttpClient httpClient = new();
                    var res = await httpClient.GetAsync(url);
                    var content = await res.Content.ReadAsStringAsync();
                    var data = JObject.Parse(content);
                    var code = data["code"]!.ToString();
                    if (code != "0") continue;
                    var list = JArray.FromObject(data["data"]!["cards"]!);
                    foreach (JObject blog in list)
                    {
                        var timestamp = blog["desc"]!["timestamp"]!.ToString().Tolong();
                        DateTime createDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        createDate = createDate.AddSeconds(timestamp).ToLocalTime();

                        if (createDate >= DateTime.Now.AddMinutes(-TimeSpan))
                        {
                            //可以认定为新发的动态
                            //获取微博类型0-视频，1-图文
                            var type = -1;
                            if (blog["desc"]!["type"]!.ToString() == "2") type = 1;
                            else if (blog["desc"]!["type"]!.ToString() == "8") type = 0;
                            //需要发送通知则发送通知
                            if (index == 0)
                            {
                                if (Const.ConfigModel.WB.forwardGroup || Const.ConfigModel.WB.forwardQQ)
                                {
                                    var mcb = new MessageChainBuilder();
                                    //预留是否要at所有人
                                    //if (false)
                                    //{
                                    //    mcb.AtAll();
                                    //}
                                    mcb.Plain($"{blog["desc"]!["user_profile"]!["info"]!["uname"]}B站更新：");
                                    var card = JObject.Parse(blog["card"]!.ToString());
                                    if (type == 1)
                                    {
                                        //获取第一张图片发送
                                        var imgList = JArray.FromObject(card!["item"]!["pictures"]!);
                                        mcb.Plain(card!["item"]!["description"]!.ToString()).ImageFromUrl(imgList[0]["img_src"]!.ToString());
                                    }
                                    else if (type == 0)
                                    {
                                        mcb.Plain(card!["title"]!.ToString())
                                            .Plain("\nBV号:" + blog["desc"]!["bvid"] + "\n直达:https://www.bilibili.com/video/" + blog["desc"]!["bvid"])
                                            .ImageFromUrl(card["pic"]?.ToString() ?? "");
                                    }
                                    else
                                    {
                                        if(card.ContainsKey("title"))
                                            mcb.Plain(card!["title"]!.ToString());
                                        else mcb.Plain(card!["item"]!["description"]!.ToString());
                                    }
                                    if (Const.ConfigModel.WB.forwardGroup)
                                    {
                                        var groups = string.IsNullOrWhiteSpace(Const.ConfigModel.WB.group) ? Const.ConfigModel.QQ.group : Const.ConfigModel.WB.group;
                                        var glist = groups.ToListV2();
                                        foreach (var group in glist)
                                        {
                                            await Msg.SendGroupMsg(group, mcb.Build());
                                        }
                                    }
                                    if (Const.ConfigModel.WB.forwardQQ)
                                    {
                                        var qqs = string.IsNullOrWhiteSpace(Const.ConfigModel.WB.qq) ? Msg.Admin : Const.ConfigModel.WB.qq;
                                        var qlist = qqs.ToListV2();
                                        foreach (var qq in qlist)
                                        {
                                            await Msg.SendFriendMsg(qq, mcb.Build());
                                        }
                                    }
                                }
                            }
                            //保存图片
                            if (type == 1)
                            {
                                var card = JObject.Parse(blog["card"]!.ToString());
                                var picList = JArray.FromObject(card!["item"]!["pictures"]!);
                                if (picList == null) continue;
                                foreach (var pic in picList)
                                {
                                    var imgUrl = pic["img_src"]!.ToString();
                                    await Weibo.FatchFace(imgUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.AddLog();
                return;
            }
        }
    }
}
