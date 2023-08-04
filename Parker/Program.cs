using NetDimension.NanUI;

namespace ParkerBot
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            WinFormium.CreateRuntimeBuilder(env =>
            {
                env.CustomCefSettings(settings =>
                {
                    // 在此处设置 CEF 的相关参数
                });

                env.CustomCefCommandLineArguments(commandLine =>
                {
                    // 在此处指定 CEF 命令行参数
                });
            }, app =>
            {
                app.UseDebuggingMode();
                var path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot");
                app.UseEmbeddedFileResource("http", "parkerbot", "wwwroot/pages", fallback => "index.html");
                app.UseLocalFileResource("http", "parkerbot.file", path);
                app.UseDataServiceResource("http", "parkerbot.api");
                // 指定启动窗体
                app.UseMainWindow(context => new Main());
            }).Build().Run();
        }
    }
}