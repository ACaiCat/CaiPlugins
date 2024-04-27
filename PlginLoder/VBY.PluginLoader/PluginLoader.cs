using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NuGet.Protocol;
using Rests;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace VBY.PluginLoader
{
	[ApiVersion(2, 1)]
	public class PluginLoader : TerrariaPlugin
	{
		private Command AddCommand;

		private List<WeakReference> OldLoaders = new List<WeakReference>();

		private MyAssemblyLoadContext Loader;

		internal static string ConfigPath = Path.Combine(TShock.SavePath, typeof(PluginLoader).Namespace + ".json");

		internal static string PluginPath = Path.Combine(TShock.SavePath, "PluginLoader");

		public int LoaderNum;

		public override string Name => GetType().Name;

		public override string Author => "yu (Cai魔改)";

		public override Version Version => GetType().Assembly.GetName().Version;

		public override string Description => "可卸载插件加载器(插件调试)";

		public PluginLoader(Main game)
			: base(game)
		{
			Loader = new MyAssemblyLoadContext("VBY.PluginLoader" + LoaderNum++);
			AddCommand = new Command(GetType().Namespace!.ToLower(), Ctl, "load");
            if (!File.Exists(ConfigPath))
			{
				File.WriteAllText(ConfigPath, JsonConvert.SerializeObject((object)new Config(), (Formatting)1));
			}
			MyAssemblyLoadContext.LoadFromDefault = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath)).LoadFromDefault;
		}

		public override void Initialize()
		{
            Directory.CreateDirectory(PluginPath);
            Commands.ChatCommands.Add(AddCommand);
			Loader.LoadPlugin(TSPlayer.Server);
            TShock.RestApi.Register(new SecureRestCommand("/pluginloder/reloaddebug", ReloadDebugPlugin, RestPermissions.restmanage));
        }

        private object ReloadDebugPlugin(RestRequestArgs args)
        {
			Loader.ReLoadDebugPlugin(args.Parameters["path"]);


            return new RestObject()
            {
                {"response", "Successs!"}
            };
        }


        protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Commands.ChatCommands.Remove(AddCommand);
				Loader.UnloadPlugin(TSPlayer.Server);
			}
			base.Dispose(disposing);
		}

		private void Ctl(CommandArgs args)
		{
			CommandArgs args2 = args;
			if (!args2.Parameters.Any())
			{
				args2.Player.SendInfoMessage("/load load load PluginLoader\n/load unload unload PluginLoader\n/load reload unload then load PluginLoader\n/load clear clear old PluginLoader and reset num");
				return;
			}
			_ = args2.Parameters.Count;
			switch (args2.Parameters[0])
			{
			case "load":
				if (Loader.Assemblies.Any())
				{
					args2.Player.SendInfoMessage("have assembly, don't load");
				}
				else
				{
					Loader.LoadPlugin(args2.Player);
				}
				break;
			case "unload":
				if (!Loader.Assemblies.Any())
				{
					args2.Player.SendInfoMessage("don't have assembly, don't unload");
					break;
				}
				Loader.UnloadPlugin(args2.Player);
				OldLoaders.Add(new WeakReference(Loader));
				Loader = new MyAssemblyLoadContext("VBY.PluginLoader" + LoaderNum++);
				break;
			case "reload":
				Loader.UnloadPlugin(args2.Player);
				OldLoaders.Add(new WeakReference(Loader));
				Loader = new MyAssemblyLoadContext("VBY.PluginLoader" + LoaderNum++);
				Loader.LoadPlugin(args2.Player);
				break;
			case "clear":
			{
				int num = 0;
				while (OldLoaders.All((WeakReference x) => x.IsAlive) && num < 10)
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					num++;
				}
				OldLoaders.RemoveAll((WeakReference x) => !x.IsAlive);
				LoaderNum = OldLoaders.Count - 1;
				args2.Player.SendInfoMessage("current active loader count:{0}", OldLoaders.Count);
				break;
			}
			case "info":
				Loader.Assemblies.ToList().ForEach(delegate(Assembly x)
				{
					args2.Player.SendInfoMessage("Assembly:{0}", x.GetName().FullName);
				});
				args2.Player.SendInfoMessage("current active old loader count:{0}", OldLoaders.Count);
				OldLoaders.Where((WeakReference x) => x.IsAlive).ForEach(delegate(WeakReference x)
				{
					if (x.Target != null)
					{
						MyAssemblyLoadContext myAssemblyLoadContext = (MyAssemblyLoadContext)x.Target;
						Console.WriteLine(myAssemblyLoadContext.Name);
						Console.WriteLine(string.Join("\n", myAssemblyLoadContext.Assemblies.Select((Assembly x) => x.FullName)));
					}
				});
				break;
			default:
				args2.Player.SendInfoMessage("unknock subcmd {0}", args2.Parameters[0]);
				break;
			}
		}
	}
}
