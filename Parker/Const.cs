using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace ParkerBot
{
    public static class Const
    {
        public static MemoryCache cache = new(new MemoryCacheOptions() { });
        public static JObject Mirai { get { return GetConfig("Mirai"); } }
        public static JObject Config { get { return GetConfig("BaseConfig"); } }
        public static Mirai? _MiraiConfig = null;
        public static Mirai MiraiConfig
        {
            get
            {
                if (_MiraiConfig == null) _MiraiConfig = Mirai.ToObject<Mirai>() ?? new();
                return _MiraiConfig;
            }
        }
        public static BaseConfig? _ConfigModel = null;
        public static BaseConfig ConfigModel
        {
            get
            {
                if (_ConfigModel == null) _ConfigModel = Config.ToObject<BaseConfig>() ?? new();
                return _ConfigModel;
            }
        }
        public static JObject Enable { get { return GetConfig("EnableModule"); } }
        public static EnableModule? _EnableModule = null;
        public static EnableModule EnableModule
        {
            get
            {
                if (_EnableModule == null) _EnableModule = Enable.ToObject<EnableModule>() ?? new();
                return _EnableModule;
            }
        }
        public static JObject GetConfig(string key)
        {
            if (cache.TryGetValue(key, out JObject? outRes))
            {
                outRes ??= new();
                return outRes;
            }
            return new();
        }

        public static void GetEnableModule()
        {
            var context = new LiteContext();
            var list = context.Config.Where(t => t.parentId == 13).ToList();
            JObject obj = new();
            foreach (var item in list)
            {
                obj.Add(item.key.ToLower(), item.value);
            }
            cache.Set("EnableModule", JObject.FromObject(obj));
        }

        public static void SetCache()
        {
            Const._MiraiConfig = null;
            Const._ConfigModel = null;
            Const._EnableModule = null;
            var context = new LiteContext();
            cache.Clear();
            var list = context.Config.ToList();
            var parent = list.Where(t => t.parentId == 0).ToList();
            foreach (var item in parent)
            {
                var ch = list.Where(t => t.parentId == item.id).ToList();
                JObject obj = new();
                foreach (var item1 in ch)
                {
                    if (!list.Any(t => t.parentId == item1.id)) obj.Add(item1.key, item1.value);
                    else obj.Add(item1.key, GetCh(list, item1));
                }
                cache.Set(item.key, obj);
            }
            Const.GetEnableModule();
        }

        private static JObject GetCh(List<Config> list, Config model)
        {
            var ch = list.Where(t => t.parentId == model.id).ToList();
            JObject obj = new();
            foreach (var item in ch)
            {
                if (!list.Any(t => t.parentId == item.id)) obj.Add(item.key, item.value);
                else obj.Add(item.key, GetCh(list, item));
            }
            return obj;
        }

        public static string FirstLow(this string str)
        {
            List<string> ignore = new() { "QQ" };
            if (ignore.Contains(str))
                return str;
            var key = str[0].ToString().ToLower();
            var key1 = str[1..];
            return key + key1;
        }

        public static int ToInt(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return 0;
            if (!int.TryParse(str, out var val)) return 0;
            return val;
        }

        public static bool ToBool(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            if (!bool.TryParse(str, out var val)) return false;
            return val;
        }
    }
}
