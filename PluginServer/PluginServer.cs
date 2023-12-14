using Mirai.Net.Data.Events;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Utils.Scaffolds;
using PluginServer;

namespace PluginServer
{
    /// <summary>
    /// 插件父类
    /// </summary>
    /// <param name="name">插件名</param>
    /// <param name="version">插件版本</param>
    /// <param name="desc">插件描述</param>
    public class BasePlugin(string name, double version, string desc) : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// 插件描述
        /// </summary>
        public string Desc { get; set; } = desc;

        /// <summary>
        /// 版本
        /// </summary>
        public double Version { get; set; } = version;

        /// <summary>
        /// 执行插件
        /// </summary>
        /// <param name="msgBase">消息基类</param>
        /// <param name="eventBase">事件基类</param>
        /// <returns></returns>
        public virtual async Task Excute(MessageReceiverBase msgBase, EventBase eventBase)
        {
            await BaseMessage(msgBase);
            await EventMessage(eventBase);
        }

        /// <summary>
        /// 执行插件
        /// </summary>
        /// <param name="msgBase">消息基类</param>
        /// <returns></returns>
        public virtual async Task Excute(MessageReceiverBase msgBase)
        {
            await BaseMessage(msgBase);
        }

        /// <summary>
        /// 执行插件
        /// </summary>
        /// <param name="eventBase">事件基类</param>
        /// <returns></returns>
        public virtual async Task Excute(EventBase eventBase)
        {
            if (eventBase != null)
            {
                await EventMessage(eventBase);
            }
        }

        /// <summary>
        /// 所有消息
        /// </summary>
        /// <param name="fmr"></param>
        /// <returns></returns>
        public virtual async Task BaseMessage(MessageReceiverBase mrb)
        {
            await Task.Delay(1);
            return;
        }

        /// <summary>
        /// 事件消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public virtual async Task EventMessage(EventBase e)
        {
            await Task.Delay(1);
            return;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~BasePlugin()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

/// <summary>
/// 使用实例
/// </summary>
/// <param name="n"></param>
/// <param name="d"></param>
/// <param name="v"></param>
class TestClass(string n, double d, string v) : BasePlugin(n, d, v)
{
    public override async Task Excute(MessageReceiverBase msgBase)
    {
        if (msgBase.Type == MessageReceivers.Friend)
        {
            var friendMsg = msgBase as FriendMessageReceiver;
            await friendMsg.SendMessageAsync("test");
        }
    }
}