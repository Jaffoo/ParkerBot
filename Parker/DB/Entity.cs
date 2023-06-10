using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkerBot
{
    [Table("message")]
    public class Message
    {
        [Key]
        public int id { get; set; }
        public string? msgId { get; set; }
        public string? fromId { get; set; }
        public string? fromName { get; set; }
        public string? toId { get; set; }
        public string? toName { get; set; }
        public string? content { get; set; }
        public string? img { get; set; }
        public string? voice { get; set; }
        public string? video { get; set; }
        public string? other { get; set; }
        public DateTime createDate { get; set; }
        /// <summary>
        /// 1-好友，2-群，3-口袋消息
        /// </summary>
        public int type { get; set; }
        //0-qq消息，其他口袋消息类型
        public int msgType { get; set; }
    }

    [Table("config")]
    public class Config
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public int parentId { get; set; }
    }

    [Table("idol")]
    public class Idol
    {
        [Key]
        public int id { get; set; }
        public string? name { get; set; }
        public string? code { get; set; }
        public string? roomId { get; set; }
        public string? serverId { get; set; }
        public string? account { get; set; }
        public string? team { get; set; }
    }

    [Table("cache")]
    public class Caches
    {
        [Key] public int id { get; set; }
        public string content { get; set; } = string.Empty;
        /// <summary>
        /// 1-图片缓存，2-已抓取的微博
        /// </summary>
        public int type { get; set; }
        public DateTime createDate { get; set; }
    }

    [Table("log")]
    public class Logs
    {
        [Key] public int id { get; set; }
        public string message { get; set; } = string.Empty;
        public DateTime createDate { get; set; }
    }
}
