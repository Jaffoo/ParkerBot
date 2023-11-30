using Mirai.Net.Data.Events;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Receivers;

namespace PluginServer
{
    public interface IBasePlugin : IDisposable
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 插件描述
        /// </summary>
        string Desc { get; }

        /// <summary>
        /// 执行插件
        /// </summary>
        /// <param name="msgBase">消息基类</param>
        /// <param name="eventBase">事件基类</param>
        /// <returns></returns>
        Task Excute(MessageReceiverBase? msgBase = null, EventBase? eventBase = null);
    }
    /// <summary>
    /// 群接口
    /// </summary>
    public interface IGroupPlugin : IBasePlugin
    {
        /// <summary>
        /// 群消息接收触发事件
        /// </summary>
        /// <param name="gmr">群消息接收器</param>
        /// <param name="msgText">纯文本消息</param>
        Task GroupExecute(GroupMessageReceiver gmr, string msgText = "");
    }

    /// <summary>
    /// 好友接口
    /// </summary>
    public interface IFriendPlugin : IBasePlugin
    {
        /// <summary>
        /// 好友消息接收触发事件
        /// </summary>
        /// <param name="fmr">好友消息接收器</param>
        /// <param name="msgText">纯文本消息</param>
        Task FriendExecute(FriendMessageReceiver fmr, string msgText = "");
    }

    /// <summary>
    /// 事件接口
    /// </summary>
    public interface IEventPlugin : IBasePlugin
    {
        /// <summary>
        /// 事件触发事件
        /// </summary>
        /// <param name="e">事件基类</param>
        Task EventExecute(EventBase e);
    }
}
