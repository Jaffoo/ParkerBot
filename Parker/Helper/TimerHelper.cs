using FluentScheduler;
using Newtonsoft.Json.Linq;
using ParkerBot;
using System.Xml.Linq;

namespace Helper
{
    public class FluentSchedulerFactory : Registry
    {
        public FluentSchedulerFactory()
        {
            if (Const.EnableModule.wb)
            {
                Schedule(async () => await Weibo.Seve()).WithName("WB").NonReentrant().ToRunEvery(Const.ConfigModel.WB.timeSpan.ToInt()).Minutes();
                Schedule(async () => await Weibo.ChiGua()).WithName("WBChiGua").NonReentrant().ToRunEvery(Const.ConfigModel.WB.timeSpan.ToInt()).Minutes();
            }
            if (Const.EnableModule.bz)
            {
                Schedule(async () => await Bilibili.Monitor()).WithName("BZ").NonReentrant().ToRunEvery(Const.ConfigModel.WB.timeSpan.ToInt()).Minutes();
            }
            if (Const.EnableModule.xhs)
            {

            }
            if (Const.EnableModule.dy)
            {

            }
            Schedule(async () => await AsyncXox()).ToRunNow().AndEvery(1).Days().At(0, 0);
        }

        public static async Task AsyncXox()
        {
            LiteContext liteContext = new();
            string url = @"https://fastly.jsdelivr.net/gh/duan602728596/qqtools@main/packages/NIMTest/node/roomId.json";
            try
            {
                HttpClient client = new();
                var str = client.GetStringAsync(url).Result;
                if (str == null) return;
                var res = JObject.Parse(str);
                var arr = JArray.FromObject(res["roomId"]!);
                foreach (var item in arr)
                {
                    Idol idol = new()
                    {
                        id = item["id"]!.ToString(),
                        name = item["ownerName"]?.ToString(),
                        roomId = item["roomId"]?.ToString()??"",
                        account = item["account"]?.ToString()??"",
                        serverId = item["serverId"]?.ToString(),
                        team = item["team"]?.ToString(),
                        teamId = item["teamId"]?.ToInt(),
                        liveId = item["liveRoomId"]?.ToString(),
                        groupName = item["groupName"]?.ToString(),
                        periodName = item["periodName"]?.ToString(),
                        pinyin = item["pinyin"]?.ToString(),
                        channelId = item["channelId"]?.ToString(),
                    };
                    var model = liteContext.Idol.FirstOrDefault(t => t.id == idol.id);
                    if (model == null)
                        await liteContext.Idol.AddAsync(idol);
                    else
                        liteContext.Entry(model).CurrentValues.SetValues(idol);
                    await liteContext.SaveChangesAsync();
                }
                await liteContext.DisposeAsync();
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
