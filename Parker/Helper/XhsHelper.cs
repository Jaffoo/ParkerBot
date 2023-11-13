using HtmlAgilityPack;
using Mirai.Net.Utils.Scaffolds;
using Newtonsoft.Json.Linq;
using ParkerBot;
using System.IO.Compression;
using System.Text;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Helper
{
    public class Xhs
    {
        public static LiteContext? dbContext;
        public static List<string> Uids => Const.ConfigModel.XHS.url.ToListV2();
        public static int TimeSpan => Const.ConfigModel.WB.timeSpan.ToInt();
        private static Dictionary<string, string> GetHeader(string uid)
        {
            return new Dictionary<string, string>()
            {
                {":authority","www.xiaohongshu.com" },
                {":method","GET" },
                {":path","/user/profile/"+uid },
                {":scheme","https" },
                {"Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7" },
                {"Accept-Encoding","gzip, deflate, br" },
                {"Accept-Language","zh-CN,zh;q=0.9" },
                {"Cache-Control","max-age=0" },
                {"Cookie","abRequestId=7d588f80-219d-5227-b1e8-9ef14746e504; webBuild=3.15.5; xsecappid=xhs-pc-web; a1=18bc6ce776fhc0tdaj76tjpj4ricat1yboom8h2ly50000366008; webId=3952f67d2805670c7109d0af98449541; gid=yYDSKSdj4fDSyYDSKSdWWlxJKixS89f0VWK9l1vVUV4F3628S09yvY888qKK88Y8SKKYJyd8; web_session=030037a262c9216f033c19db29224a9b9d681f" },
                {"Sec-Ch-Ua","\"Chromium\";v=\"118\", \"Google Chrome\";v=\"118\", \"Not=A?Brand\";v=\"99\"" },
                {"Sec-Ch-Ua-Mobile","?0" },
                {"ec-Ch-Ua-Platform","\"Windows\"" },
                {"Sec-Fetch-Dest","document" },
                {"Sec-Fetch-Mode","navigate" },
                {"Sec-Fetch-Site","same-origin" },
                {"Sec-Fetch-User","?1" },
                {"Upgrade-Insecure-Requests","1" },
                {"User-Agent","Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36" },
            };
        }

        public static async Task Save()
        {
            string url = "";
            try
            {
                foreach (var item in Uids)
                {
                    url = "https://www.xiaohongshu.com/user/profile/" + item;
                    var handler = new HttpClientHandler() { UseCookies = true };
                    HttpClient httpClient = new(handler);
                    var headers = GetHeader(item);
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                    var res = await httpClient.SendAsync(request);
                    var stream = await res.Content.ReadAsStreamAsync();
                    var html = Encoding.UTF8.GetString(Decompress(stream));
                    HtmlDocument doc = new();
                    doc.LoadHtml(html);
                    HtmlNode? scriptNode = doc.DocumentNode.Descendants("script")
                    .FirstOrDefault(n => n.InnerText.Contains("window.__INITIAL_STATE__"));
                    if (scriptNode == null) continue;
                    var text = scriptNode.InnerText;
                    var jsonObj = text.Replace("window.__INITIAL_STATE__=", "");
                    JObject obj = JObject.Parse(jsonObj);
                    var notesStr = obj["user"]?["notes"]?[0]?.ToString();
                    if (string.IsNullOrWhiteSpace(notesStr)) continue;
                    var notes = JArray.Parse(notesStr);
                    if (notes == null) continue;
                    var model = notes.Where(t => t["noteCard"]!["interactInfo"]!["sticky"]!.Value<bool>() != true).FirstOrDefault();
                    if (model == null) continue;
                    var noteUrl = "https://www.xiaohongshu.com/explore/" + model["id"];
                    request = new HttpRequestMessage(HttpMethod.Get, noteUrl);
                    res = await httpClient.SendAsync(request);
                    html = await res.Content.ReadAsStringAsync();
                    doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    scriptNode = doc.DocumentNode.Descendants("script")
                   .FirstOrDefault(n => n.InnerText.Contains("window.__INITIAL_STATE__"));
                    if (scriptNode == null) continue;
                    text = scriptNode.InnerText;
                    jsonObj = text.Replace("window.__INITIAL_STATE__=", "");
                    obj = JObject.Parse(jsonObj);
                    var noteStr = obj["note"]!["noteDetailMap"]![model["id"]!.ToString()]!["note"]!.ToString();
                    if (string.IsNullOrWhiteSpace(noteStr)) continue;
                    var note = JObject.Parse(noteStr);
                    var timestamp = Convert.ToInt64(note["time"]!);
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                    // 转换为本地时间
                    DateTime createDate = dateTimeOffset.LocalDateTime;
                    if (createDate < DateTime.Now.AddMinutes(-TimeSpan)) continue;
                    //可以认定为是新发的笔记
                    //type=1-图片，type=2-视频
                    var type = note["type"]!.ToString() == "normal" ? 1 : 2;
                    var first = "";
                    if (type == 1)
                    {
                        //图片
                        var imageList = JArray.Parse(note["imageList"]!.ToString());
                        first = imageList[0]!["infoList"]?[1]?["url"]?.ToString();
                        //保存阿里云盘
                        foreach (var pic in imageList)
                        {
                            var imgUrl = pic["infoList"]?[1]?["url"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(imgUrl))
                                await Weibo.FatchFace(imgUrl, true);
                        }
                    }
                    else
                    {
                        //视频
                    }
                    //发送消息
                    var str = $"{note["user"]!["nickname"]!}发小红书啦！\n类型：图片\n标题：{note["title"]}\n内容：{note["desc"]}";
                    if (Const.WindStatus)
                    {
                        MsgModel msgModel = new();
                        msgModel.MsgStr = noteUrl;
                        if (type == 1)
                        {
                            msgModel.Type = 3;
                            msgModel.MsgStr += str;
                            msgModel.Url = !string.IsNullOrWhiteSpace(first) ? FileHelper.SaveLocal(first) : "";
                        }
                        else
                        {
                            msgModel.Type = 0;
                            msgModel.MsgStr += str;
                        }
                        if (Const.ConfigModel.XHS.forwardGroup) msgModel.AddMsg();
                    }
                    else
                    {
                        var mcb = new MessageChainBuilder();
                        mcb.Plain(noteUrl).Plain(str);
                        if (type == 1) mcb.ImageFromUrl(first);
                        if (Const.ConfigModel.XHS.forwardGroup)
                        {
                            var groups = string.IsNullOrWhiteSpace(Const.ConfigModel.XHS.group) ? Const.ConfigModel.QQ.group : Const.ConfigModel.XHS.group;
                            var glist = groups.ToListV2();
                            foreach (var group in glist)
                            {
                                await Msg.SendGroupMsg(group, mcb.Build());
                            }
                        }
                        if (Const.ConfigModel.XHS.forwardQQ)
                        {
                            var qqs = string.IsNullOrWhiteSpace(Const.ConfigModel.XHS.qq) ? Msg.Admin : Const.ConfigModel.XHS.qq;
                            var qlist = qqs.ToListV2();
                            foreach (var qq in qlist)
                            {
                                await Msg.SendFriendMsg(qq, mcb.Build());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.AddLog("报错链接：" + url + "\n错误信息：" + ex.Message);
                return;
            }
        }
        public static byte[] Decompress(Stream stream)
        {
            var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            var mmStream = new MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int byteRead = gzipStream.Read(block, 0, block.Length);
                if (byteRead <= 0)
                    break;
                else
                    mmStream.Write(block, 0, byteRead);
            }
            mmStream.Close();
            return mmStream.ToArray();
        }
    }
}