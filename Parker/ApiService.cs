using Helper;
using Manganese.Text;
using NetDimension.NanUI.Browser.ResourceHandler;
using NetDimension.NanUI.Resource.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static ParkerBot.Const;

namespace ParkerBot
{
    public class ApiService : DataService
    {
        [RouteGet]
        public ResourceResponse GetMiraiFile(ResourceRequest request)
        {
            try
            {
                string filePath = Const.Mirai["Path"]!.ToString();
                if (!Directory.Exists(filePath)) return Json(false);
                filePath = Path.Combine(filePath, @"config/net.mamoe.mirai-api-http/setting.yml");
                string yamlContent = File.ReadAllText(filePath);
                yamlContent = yamlContent.Trim().Replace("\n", "");
                Regex regex = new("(?<=verifyKey: ).*?(?=##)");
                var verifyKey = regex.Match(yamlContent).Value;
                regex = new("(?<=host: ).*?(?=port)");
                var host = regex.Matches(yamlContent)[0].Value.Trim();
                regex = new("(?<=port: ).*?(?=cors)");
                var port = regex.Match(yamlContent).Value.Trim();
                if (host == "0.0.0.0") host = "localhost";
                var address = host + ":" + port;
                var obj = new
                {
                    verifyKey,
                    address,
                };
                return Json(obj);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [RouteGet]
        public ResourceResponse StartMiraiConsole(ResourceRequest request)
        {
            try
            {
                string filePath = Const.Mirai["Path"]!.ToString();
                if (!Directory.Exists(filePath)) return Json(false);
                if (!File.Exists(filePath + "/mcl.cmd")) return Json(false);
                Process process = new()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        FileName = "cmd.exe",
                        Arguments = $"/k cd {filePath} && mcl.cmd",
                    }
                };
                process.Start();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        [RouteGet]
        public ResourceResponse GetMiraiConfig(ResourceRequest request)
        {
            var dbContext = new LiteContext();
            var useMirai = Const.Mirai["UseMirai"]!.ToString().ToBool();
            var path = Const.Mirai["Path"]!.ToString();
            var address = Const.Mirai["Address"]!.ToString();
            var qq = Const.Mirai["QQNum"]!.ToString();
            var verifyKey = Const.Mirai["VerifyKey"]!.ToString();
            var pId = dbContext.Config.FirstOrDefault(t => t.key == "BaseConfig")?.id ?? 0;
            object obj = new
            {
                useMirai,
                path,
                address,
                qq,
                verifyKey,
            };
            return Json(obj);
        }

        [RouteGet]
        public ResourceResponse GetBaseConfig(ResourceRequest request)
        {
            var config = (JObject)Const.Config.DeepClone();
            dynamic data = new System.Dynamic.ExpandoObject();
            var dbContext = new LiteContext();
            var pId = dbContext.Config.FirstOrDefault(t => t.key == "BaseConfig")?.id ?? 0;
            var list = dbContext.Config.Where(t => t.parentId == pId).ToList();
            data.enable = list;

            if (string.IsNullOrWhiteSpace(config["BD"]!["ImageList"]?.ToString()))
            {
                config["BD"]!["ImageList"] = new JArray();
            }
            else
            {
                var imageList = config["BD"]!["ImageList"]!.ToString().Split(",").ToArray();
                config["BD"]!["ImageList"] = JArray.FromObject(imageList);
            }
            var model = ConfigModel;
            if (!string.IsNullOrWhiteSpace(model.BD.imageList))
            {
                var list1 = model.BD.imageList.ToListV2().Select(t => new
                {
                    name = t.Replace("/images/standard", ""),
                    url = "http://parkerbot.file" + t
                } as object).ToList();
                model!.BD.imageList1 = list1;
            }
            data.config = model;
            data.mirai = Const.Mirai.ToObject<Mirai>();
            return Json(data);
        }

        [RouteGet]
        public ResourceResponse GetQQFun(ResourceRequest request)
        {
            var dbContext = new LiteContext();
            var pid = dbContext.Config.FirstOrDefault(t => t.key == "QQFuncList")?.id ?? 0;
            var funList = dbContext.Config.Where(t => t.parentId == pid).ToList();
            return Json(funList);
        }

        [RoutePost]
        public ResourceResponse SetMiraiConfig(ResourceRequest request)
        {
            var dbContext = new LiteContext();
            var tran = dbContext.Database.BeginTransaction();
            try
            {
                var parent = dbContext.Config.FirstOrDefault(t => t.key == "Mirai");
                var body = request.StringContent;
                var data = JObject.Parse(body);
                var chidren = dbContext.Config.Where(t => t.parentId == parent!.id).ToList();
                chidren.FirstOrDefault(t => t.key == "UseMirai")!.value = data["useMirai"]?.ToString() ?? "false";
                chidren.FirstOrDefault(t => t.key == "Path")!.value = data["path"]?.ToString() ?? "";
                chidren.FirstOrDefault(t => t.key == "QQNum")!.value = data["qq"]?.ToString() ?? "";
                chidren.FirstOrDefault(t => t.key == "VerifyKey")!.value = data["verifyKey"]?.ToString() ?? "";
                chidren.FirstOrDefault(t => t.key == "Address")!.value = data["address"]?.ToString() ?? "";
                dbContext.Config.UpdateRange(chidren);
                dbContext.SaveChanges();
                tran.Commit();
                SetCache();
                return Json(true);
            }
            catch (Exception)
            {
                tran.Rollback();
                return Json(false);
            }
        }

        [RouteGet]
        public ResourceResponse StartAliYunApi(ResourceRequest request)
        {
            Task.Run(() =>
            {
                if (Const.EnableModule.bd && ConfigModel.BD.saveAliyunDisk)
                {
                    var path = Directory.GetCurrentDirectory() + "/wwwroot/script/AliDiskApi.exe";
                    using Process p = Process.Start(path)!;
                }
            });
            return Json(null);
        }

        [RoutePost]
        public ResourceResponse SetConfig(ResourceRequest request)
        {
            var dbContext = new LiteContext();
            var tran = dbContext.Database.BeginTransaction();
            var body = request.StringContent;
            var data = JObject.Parse(body);
            try
            {
                Config model = new();
                List<Config> chidren = new();
                List<Config> updataList = new();
                List<string> mods = dbContext.Config.Where(t => t.parentId == 13).Select(t => t.key!).ToList();
                foreach (var mod in mods)
                {
                    model = dbContext.Config.FirstOrDefault(t => t.key == mod)!;
                    model.value = data["enable"]![mod.ToLower()]!.ToString().ToLower();
                    if (!bool.Parse(model.value)) continue;
                    chidren = dbContext.Config.Where(t => t.parentId == model.id).ToList();
                    chidren.ForEach(item =>
                    {
                        item.value = data["config"]![mod]![item.key!.FirstLow()]?.ToString() ?? "";
                        if (item.key == "ImageList")
                        {
                            item.value = item.value.Replace("http://parkerbot.file", "");
                        }
                        if (item.value == "True" || item.value == "False")
                        {
                            item.value = item.value.ToLower();
                        }
                    });
                    updataList.Add(model);
                    updataList.AddRange(chidren);
                }
                dbContext.Config.UpdateRange(updataList);
                dbContext.SaveChanges();
                tran.Commit();
                SetCache();
                return Json(true);
            }
            catch (Exception)
            {
                tran.Rollback();
                return Json(false);
            }
        }

        [RoutePost]
        public ResourceResponse Upload(ResourceRequest request)
        {
            var path = request.UploadFiles[0];
            FileInfo fileInfo = new(path);
            var root = Directory.GetCurrentDirectory();
            root = Path.Combine(root, "wwwroot/images/standard");
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var name = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var fileName = "/" + name + fileInfo.Extension;
            if (!File.Exists(root + fileName))
                fileInfo.CopyTo(root + fileName);
            else
            {
                File.Delete(root + fileName);
                fileInfo.CopyTo(root + fileName);
            }
            object obj = new
            {
                name,
                url = "/images/standard" + fileName
            };
            return Json(obj);
        }

        [RouteGet]
        public ResourceResponse Start(ResourceRequest request)
        {
            if (!StartBot.HasWebSocket) new StartBot().Start();
            if (!StartMirai.HasMirai) new StartMirai();
            StartTimer.Start();
            Thread.Sleep(5000);
            var obj = new
            {
                pocket = StartBot.HasWebSocket,
                mirai = StartMirai.HasMirai
            };
            return Json(obj);
        }

        [RouteGet]
        public ResourceResponse Refresh(ResourceRequest request)
        {
            var dbContext = new LiteContext();
            var list = dbContext.Caches.Where(t => t.type == 1).ToList();
            list.ForEach(item =>
            {
                if (!item.content.Contains("cdn.ipfsscan.io") && item.content.Contains("sinaimg.cn"))
                {
                    var fileName = Path.GetFileName(item.content);
                    item.content = "https://cdn.ipfsscan.io/weibo/large/" + fileName;
                }
                var list = new List<string> { ".jpg", ".JPG", ".jpeg", ".JPEG", ".png", ".PNG" };
                if (!list.Any(item.content.Contains))
                {
                    item.content = Base64Helper.UrlImgToBase64(item.content).Result;
                }
            });
            return Json(list);
        }

        [RouteGet]
        public ResourceResponse PicCheck(ResourceRequest request)
        {
            var b = false;
            var dbContext = new LiteContext();
            var id = request.QueryString["id"]!.ToInt32();
            var type = request.QueryString["type"]!.ToInt32();
            var model = dbContext.Caches.FirstOrDefault(t => t.id == id);
            if (model == null) return Json(b);
            if (type == 1)
            {
                b = FileHelper.Save(model.content);
                if (b)
                {
                    dbContext.Caches.Remove(model);
                    b = dbContext.SaveChanges() > 0;
                }
            }
            if (type == 2)
            {
                dbContext.Caches.Remove(model);
                b = dbContext.SaveChanges() > 0;
            }
            return Json(b);
        }

        [RouteGet]
        public ResourceResponse GetXox(ResourceRequest request)
        {
            var group = request.QueryString["group"]?.ToString()?.Split(",").ToList();
            var name = request.QueryString["name"]!.ToString();
            string url = @"https://fastly.jsdelivr.net/gh/duan602728596/qqtools@main/packages/NIMTest/node/roomId.json";
            try
            {
                LiteContext dbContext = new();
                Idol? xox = new();
                IQueryable<Idol>? chain = null;
                if (group != null)
                {
                    if (group.Count >= 2)
                        chain = dbContext.Idol.Where(t => t.groupName == group[0] && t.team == group[1]);
                    else
                        chain = dbContext.Idol.Where(t => t.groupName == group[0]);
                }
                xox = chain?.FirstOrDefault(t => t.name == name);
                if (xox == null) return Json(new { success = false, msg = "未查询到该小偶像", data = url });
                return Json(new { success = true, msg = "", data = xox.ToString() });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message, data = url });
            }
        }
    }
}
