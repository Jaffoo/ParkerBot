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
                if (!Const.EnableModule.qq && !Const.WindStatus) return;
                if (!UseMirai && !Const.WindStatus) return;
                Task.Factory.StartNew(() =>
                {// 设置当前线程为STA模式
                    if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                    {
                        // 设置当前线程为STA模式
                        Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                    }
                    BotStart();
                });
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
                if (Const.WindStatus)
                {
                    Msg.BotStart(null);
                    HasMirai = true;
                }
                else if (UseMirai)
                {
                    var bot = new MiraiBot
                    {
                        Address = Const.MiraiConfig.address,
                        QQ = Const.MiraiConfig.QQNum,
                        VerifyKey = Const.MiraiConfig.verifykey
                    };
                    await bot.LaunchAsync();
                    Msg.BotStart(bot);
                    HasMirai = true;
                }
                while (true)
                {
                    Thread.Sleep(10);
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
