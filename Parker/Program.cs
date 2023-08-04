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
                    // �ڴ˴����� CEF ����ز���
                });

                env.CustomCefCommandLineArguments(commandLine =>
                {
                    // �ڴ˴�ָ�� CEF �����в���
                });
            }, app =>
            {
                app.UseDebuggingMode();
                var path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot");
                app.UseEmbeddedFileResource("http", "parkerbot", "wwwroot/pages", fallback => "index.html");
                app.UseLocalFileResource("http", "parkerbot.file", path);
                app.UseDataServiceResource("http", "parkerbot.api");
                // ָ����������
                app.UseMainWindow(context => new Main());
            }).Build().Run();
        }
    }
}