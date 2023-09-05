using Fleck;
using FluentScheduler;
using Helper;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Data.Messages;
using Mirai.Net.Sessions;
using Newtonsoft.Json.Linq;
using System.Reactive.Linq;

namespace ParkerBot
{
    public class StartBot
    {
        public WebSocketServer? WebSocket { get; set; }
        public static bool HasWebSocket { get; set; } = false;
        public bool Start()
        {
            try
            {
                if (!Const.EnableModule.kd) return false;
                WebSocket = new WebSocketServer("ws://0.0.0.0:6001");
                WebSocket.Start(socket =>
                {
                    socket.OnMessage = async msg =>
                    {
                        //1-房间消息 2-直播消息
                        var fromType = (JObject.Parse(msg)["fromType"]?.ToString() ?? "1").ToInt();
                        if (fromType == 1)
                            await Pocket.PocketMessageReceiver(msg);
                        if (fromType == 2)
                            Pocket.LiveMsgReceiver(msg);
                    };
                });
                HasWebSocket = true;
                return true;
            }
            catch (Exception)
            {
                HasWebSocket = false;
                return false;
            }
        }
    }

    public class StartMirai
    {
        public static bool HasMirai { get; set; } = false;
        public bool UseMirai
        {
            get { return Const.MiraiConfig.useMirai; }
        }
        public StartMirai()
        {
            try
            {
                if (!Const.EnableModule.qq) return;
                if (!UseMirai) return;
                Task.Run(BotStart);
            }
            catch (Exception)
            {
                HasMirai = false;
                return;
            }
        }
        public async void BotStart()
        {
            try
            {
                using var bot = new MiraiBot
                {
                    Address = Const.MiraiConfig.address,
                    QQ = Const.MiraiConfig.QQNum,
                    VerifyKey = Const.MiraiConfig.verifykey
                };
                await bot.LaunchAsync();
                Msg.BotStart(bot);
                HasMirai = true;
                while (true)
                {
                    Thread.Sleep(1);
                }
            }
            catch (Exception)
            {
                HasMirai = false;
            }
        }
    }

    public class StartTimer
    {
        public static void Start()
        {
            Task.Run(() =>
            {
                JobManager.RemoveAllJobs();
                JobManager.Initialize(new FluentSchedulerFactory());
            });
        }
    }
}
