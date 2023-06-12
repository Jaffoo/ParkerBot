using Fleck;
using FluentScheduler;
using Helper;
using Mirai.Net.Sessions;
using System.Diagnostics;

namespace ParkerBot
{
    public class StartBot
    {
        public WebSocketServer? WebSocket { get; set; }
        public static bool HasWebSocket { get; set; } = false;
        public bool Start()
        {
            //var a = Base64.UrlImgToBase64("http://parkerbot.file/images/66-230612224035866.jpeg").Result;
            try
            {
                if (Const.EnableModule.bd && Const.ConfigModel.BD.saveAliyunDisk)
                {
                    Task.Run(StartProcess);
                }
                if (!Const.EnableModule.kd) return false;
                WebSocket = new WebSocketServer("ws://0.0.0.0:6001");
                WebSocket.Start(socket =>
                {
                    socket.OnMessage = async msg =>
                    {
                        await Pocket.PocketMessageReceiver(msg);
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
        private void StartProcess()
        {
            lock (this)
            {
                var path = Directory.GetCurrentDirectory() + "/wwwroot/script/AliDiskApi.exe";
                using Process p = Process.Start(path)!;
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
                Task.Run(async () =>
                {
                    using var bot = new MiraiBot
                    {
                        Address = Const.MiraiConfig.address,
                        QQ = Const.MiraiConfig.QQNum,
                        VerifyKey = Const.MiraiConfig.verifykey
                    };
                    await bot.LaunchAsync();
                    Msg.BotStart(bot);
                    while (true)
                    {
                        Thread.Sleep(1);
                    }
                });
                HasMirai = true;
            }
            catch (Exception)
            {
                HasMirai = false;
                return;
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
