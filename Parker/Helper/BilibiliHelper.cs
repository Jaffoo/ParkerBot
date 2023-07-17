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
            get { return (Const.ConfigModel.BZ.url ?? "").Split(",").ToList(); }
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
                    var url = "https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space?host_mid=" + item;
                    var handler = new HttpClientHandler() { UseCookies = true };
                    HttpClient httpClient = new(handler);
                    httpClient.DefaultRequestHeaders.Add("Cookie", @"buvid3=58F14D89-11DF-647C-3AC6-8F15F22713D809834infoc; b_nut=1689419709; b_lsid=A6A7F965_1895942FF52; _uuid=109EA106B5-A6C8-D325-213F-724EACB7D61610296infoc; buvid_fp=d41d346d3dbd68a9e773c53562f86fe6; buvid4=C1503B9C-500E-4F3B-7D70-0B793816F3CF11282-023071519-Mdlkjl0/Y/VGNZRRioOIsA%3D%3D; PVID=1");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 Edg/114.0.1823.79");
                    httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\" Not A; Brand\";v=\"99\", \"Chromium\";v=\"101\", \"Microsoft Edge\";v=\"101\"");
                    httpClient.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "Windows");
                    var res = await httpClient.GetAsync(url);
                    var content = await res.Content.ReadAsStringAsync();
                    var data = JObject.Parse(content);
                    var code = data["code"]!.ToString();
                    if (code != "0") continue;
                    var list = JArray.FromObject(data["data"]!["items"]!);
                    foreach (JObject blog in list)
                    {
                        var timestamp = blog["modules"]!["module_author"]!["pub_ts"]!.ToString().Tolong();
                        DateTime createDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        createDate = createDate.AddSeconds(timestamp).ToLocalTime();

                        if (createDate >= DateTime.Now.AddMinutes(-TimeSpan))
                        {
                            //可以认定为新发的动态
                            //获取微博类型0-视频，1-图文
                            var type = -1;
                            if (blog["modules"]!["module_dynamic"]!["major"]!["type"]!.ToString() == "MAJOR_TYPE_DRAW") type = 1;
                            else if (blog["modules"]!["module_dynamic"]!["major"]!["type"]!.ToString() == "MAJOR_TYPE_ARCHIVE") type = 0;
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
                                    mcb.Plain($"{blog["modules"]!["module_author"]!["name"]}小破站发动态啦！");
                                    if (type == 1)
                                    {
                                        //获取第一张图片发送
                                        var imgList = JArray.FromObject(blog["modules"]!["module_dynamic"]!["major"]!["draw"]!["items"]!);
                                        mcb.Plain("\n" + blog["modules"]!["module_dynamic"]!["desc"]!["text"]!.ToString()).ImageFromUrl(imgList[0]["src"]!.ToString());
                                    }
                                    else if (type == 0)
                                    {
                                        var videoInfo = blog["modules"]!["module_dynamic"]!["major"]!["archive"]!;
                                        mcb.Plain("BV号:" + videoInfo["bvid"] + "\n直达:" + videoInfo["jump_url"])
                                            .Plain("\n" + videoInfo["badge"]!["text"]!).ImageFromUrl(videoInfo["cover"]?.ToString() ?? "");
                                    }
                                    else
                                    {
                                        mcb.Plain("未知类型动态，更多类型通知尽请期待！");
                                    }
                                    if (Const.ConfigModel.WB.forwardGroup)
                                    {
                                        var groups = string.IsNullOrWhiteSpace(Const.ConfigModel.WB.group) ? Const.ConfigModel.QQ.group : Const.ConfigModel.WB.group;
                                        var glist = string.IsNullOrWhiteSpace(groups) ? new List<string>() : groups.Split(",").ToList();
                                        foreach (var group in glist)
                                        {
                                            await Msg.SendGroupMsg(group, mcb.Build());
                                        }
                                    }
                                    if (Const.ConfigModel.WB.forwardQQ)
                                    {
                                        var qqs = string.IsNullOrWhiteSpace(Const.ConfigModel.WB.qq) ? Msg.Admin : Const.ConfigModel.WB.qq;
                                        var qlist = string.IsNullOrWhiteSpace(qqs) ? new List<string>() : qqs.Split(",").ToList();
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
                                var picList = JArray.FromObject(blog["modules"]!["module_dynamic"]!["major"]!["draw"]!["items"]!);
                                if (picList == null) continue;
                                foreach (var pic in picList)
                                {
                                    var imgUrl = pic["src"]!.ToString();
                                    await Weibo.FatchFace(imgUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var _context = new LiteContext();
                await _context.Logs.AddAsync(new Logs
                {
                    message = e.Message + "\n堆栈信息：\n" + e.StackTrace,
                    createDate = DateTime.Now,
                });
                var b = await _context.SaveChangesAsync();
                await _context.DisposeAsync();
                if (b > 0) await Msg.SendFriendMsg(Msg.Admin, "程序报错了，请联系反馈给开发人员！");
                else await Msg.SendFriendMsg(Msg.Admin, "日志写入失败。" + e.Message + "\n" + e.StackTrace);
                return;
            }
        }
    }
}
