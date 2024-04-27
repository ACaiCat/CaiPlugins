using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;
using System.Data;
using TShockAPI.Hooks;
using MonoMod.Cil;
using System.Diagnostics;

namespace AutoReboot
{
    [ApiVersion(2, 1)]
    public class AutoReboot : TerrariaPlugin
    {
        public AutoReboot(Terraria.Main game) : base(game) { }

        public override string Name => "AutoReboot";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override string Author => "Cai";

        public override string Description => "AutoReboot";

        public override void Initialize()
        {
            //用FileSystemWatcher检测/repos文件夹中文件及其文件夹内的文件夹的文件更新或者被添加、
            FileSystemWatcher watcher = new FileSystemWatcher(@"C:/Users/13110/source/repos/");
            //设置监视器的类型
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            //设置监视器的文件类型
            watcher.Filter = "*.dll";
            //设置监视器是否包含子目录
            watcher.IncludeSubdirectories = true;
            //设置监视器的启动状态
            watcher.EnableRaisingEvents = true;
            //设置监视器的事件
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.Created += new FileSystemEventHandler(watcher_Changed);
            watcher.Deleted += new FileSystemEventHandler(watcher_Changed);




        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            //将文件移动到ServerPlugins文件夹
            System.IO.File.Move(e.FullPath, "ServerPlugins/" + e.Name);
            //重启服务器
            Process.GetCurrentProcess().Kill();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {


            }
            base.Dispose(disposing);
        }
    }
}
