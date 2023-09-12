using NetDimension.NanUI;
using NetDimension.NanUI.HostWindow;
using NetDimension.NanUI.JavaScript;
using ParkerBot.Helper;
using System.Diagnostics;

namespace ParkerBot
{
    public class Main : Formium
    {
        // 设置窗体样式类型
        public override HostWindowType WindowType => HostWindowType.System;
        // 指定启动 Url
        public override string StartUrl
        {
            get
            {
                //if (Debugger.IsAttached)
                //    return "http://localhost:5173/";
                //else
                    return "http://parkerbot/";
            }
        }

        public Main()
        {
            Size = new Size(1024, 768);
            Sizable = false;
            Maximizable = false;
            StartPosition = FormStartPosition.CenterParent;
            EnableSplashScreen = false;
            Title = "ParkerBot";
            Icon = new Icon(Directory.GetCurrentDirectory() + "/wwwroot/image/sharpicons_Spider.ico");
        }

        protected override void OnReady()
        {
            //建立数据库
            var b = SqlHelper.CreateDBFile("config.db");
            //创建表（不存在时创建）
            //读取表结构sql
            if (b)
            {
                string path = Environment.CurrentDirectory + @"/wwwroot/sql/main.sql";
                var sql = File.ReadAllText(path);
                SqlHelper.ExecuteNonQuery(sql);
            }

            //数据库有更新时执行
            SqlUpdate.ExecuteSql();

            Const.SetCache();
            RegistJs();

            LoadEnd += PageLoadEnd;
        }

        private void PageLoadEnd(object? sender, NetDimension.NanUI.Browser.LoadEndEventArgs e)
        {
            //预留
            if (Debugger.IsAttached)
            {
                var res = QQFunction.WenAn("昆明");
            }
        }

        private void RegistJs()
        {
            var obj = new JavaScriptObject();

            RegisterJavaScriptObject("main", obj);
        }
    }
}
