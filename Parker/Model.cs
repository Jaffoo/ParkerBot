using System.ComponentModel.DataAnnotations.Schema;

namespace ParkerBot
{
    public class Mirai
    {
        public bool useMirai { get; set; }
        public string path { get; set; } = string.Empty;
        public string QQNum { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string verifykey { get; set; } = string.Empty;
    }
    public class BaseConfig
    {
        public QQ QQ { get; set; } = new();
        public WB WB { get; set; } = new();
        public WB BZ { get; set; } = new();
        public KD KD { get; set; } = new();
        public WB XHS { get; set; } = new();
        public WB DY { get; set; } = new();
        public BD BD { get; set; } = new();
    }
    public class QQ
    {
        public string funcEnable { get; set; } = string.Empty;
        public string funcAdmin { get; set; } = string.Empty;
        public string funcUser { get; set; } = string.Empty;
        public string group { get; set; } = string.Empty;
        public string admin { get; set; } = string.Empty;
        public bool notice { get; set; } = true;
        public string permission { get; set; } = string.Empty;
        public string sensitive { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty;
        public bool debug { get; set; } = false;
        public string gptKey { get; set; } = string.Empty;
    }
    public class WB
    {
        public string url { get; set; } = string.Empty;
        public string timeSpan { get; set; } = string.Empty;
        public string group { get; set; } = string.Empty;
        public bool forwardGroup { get; set; }
        public bool forwardQQ { get; set; }
        public string qq { get; set; } = string.Empty;
        public string cg { get; set; } = string.Empty;
        public string keyword { get; set; } = string.Empty;
    }

    public class KD
    {
        public string name { get; set; } = string.Empty;
        public string account { get; set; } = string.Empty;
        public string token { get; set; } = string.Empty;
        public string serverId { get; set; } = string.Empty;
        public string group { get; set; } = string.Empty;
        public bool forwardGroup { get; set; }
        public bool forwardQQ { get; set; }
        public string qq { get; set; } = string.Empty;
        public string appKey { get; set; } = string.Empty;
        public string imgDomain { get; set; } = string.Empty;
        public string mP4Domain { get; set; } = string.Empty;
        public string liveRoomId { get; set; } = string.Empty;
        public string msgType { get; set; } = string.Empty;
        public string msgTypeList { get; set; } = string.Empty;
    }
    public class BD
    {
        public string appKey { get; set; } = string.Empty;
        public string appSeret { get; set; } = string.Empty;
        public string similarity { get; set; } = string.Empty;
        public bool saveAliyunDisk { get; set; }
        public string audit { get; set; } = string.Empty;
        public string albumName { get; set; } = string.Empty;
        public bool faceVerify { get; set; }
        public string imageList { get; set; } = string.Empty;
        [NotMapped]
        public List<object> imageList1 { get; set; } = new();
    }
    public class EnableModule
    {
        public bool qq { get; set; }
        public bool wb { get; set; }
        public bool bz { get; set; }
        public bool kd { get; set; }
        public bool xhs { get; set; }
        public bool dy { get; set; }
        public bool bd { get; set; }
    }

    public class PocketMessage
    {
        public string Body { get; set; } = "";
        public string ChannelId { get; set; } = "";
        public string DeliveryStatus { get; set; } = "";
        public string FromAccount { get; set; } = "";
        public string FromClientType { get; set; } = "";
        public string FromDeviceId { get; set; } = "";
        public string FromNick { get; set; } = "";
        public bool HistoryEnable { get; set; } = false;
        public string MsgIdClient { get; set; } = "";
        public string MsgIdServer { get; set; } = "";
        public bool NeedBadge { get; set; } = false;
        public bool NeedPushNick { get; set; } = false;
        public string NotifyReason { get; set; } = "";
        public bool PushEnable { get; set; } = false;
        public bool RouteEnable { get; set; } = false;
        public string ServerId { get; set; } = "";
        public int Status { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; } = "";
        public string UdateTime { get; set; } = "";
        public Extension Ext { get; set; } = new();
        public Attach Attach { get; set; } = new();
    }
    public class Attach
    {
        public string MessageType { get; set; } = "";
        public ReplyInfo ReplyInfo { get; set; } = new();
        public ReplyInfo GiftReplyInfo { get; set; } = new();
        public CustomFileInfo AudioInfo { get; set; } = new();
        public CustomFileInfo VideoInfo { get; set; } = new();
        public LivePushInfo LivePushInfo { get; set; } = new();
        public FilpCard FilpCardInfo { get; set; } = new();
        public FilpCard FilpCardAudioInfo { get; set; } = new();
        public FilpCard FilpCardVideoInfo { get; set; } = new();
    }
    public class LivePushInfo
    {
        public string LiveId { get; set; } = "";
        public string LiveTitle { get; set; } = "";
        public string LiveCover { get; set; } = "";
        public string ShortPath { get; set; } = "";
    }
    public class ReplyInfo
    {
        public string ReplyName { get; set; } = "";
        public string ReplyText { get; set; } = "";
        public string Text { get; set; } = "";
    }
    public class CustomFileInfo
    {
        public string Url { get; set; } = "";
    }
    public class FilpCard
    {
        public string AnswerId { get; set; } = "";
        public string AnswerType { get; set; } = "";
        public string Question { get; set; } = "";
        public string QuestionId { get; set; } = "";
        public string SourceId { get; set; } = "";
        public string RoomId { get; set; } = "";
        public Answer Answer { get; set; } = new();
    }
    public class Answer
    {
        private string _url = "";
        public string Url { get { return _url; } set { _url = value + ".acc"; } }
        public string Duration { get; set; } = "";
        public string Size { get; set; } = "";
    }
    public class Extension
    {
        public string BubbleId { get; set; } = "";
        public int ChannelRole { get; set; }
        public string Md5 { get; set; } = "";
        public string Module { get; set; } = "";
        public User User { get; set; } = new();
    }
    public class User
    {
        public string UserId { get; set; } = "";
        public string Avatar { get; set; } = "";
        public string NickName { get; set; } = "";
        public string PfUrl { get; set; } = "";
        public string TeamLogo { get; set; } = "";
        public bool Vip { get; set; }
    }
}
