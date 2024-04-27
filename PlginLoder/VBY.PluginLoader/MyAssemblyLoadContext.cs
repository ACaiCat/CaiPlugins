using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace VBY.PluginLoader
{
	internal class MyAssemblyLoadContext : AssemblyLoadContext
	{
		internal static string[] LoadFromDefault = new string[5] { "TerrariaServer", "OTAPI", "OTAPI.Runtime", "ModFramework", "TShockAPI" };

		private List<TerrariaPlugin> Plugins = new List<TerrariaPlugin>();

        private List<TerrariaPlugin> TestPlugins = new List<TerrariaPlugin>();

        public MyAssemblyLoadContext(string name)
			: base(name, isCollectible: true)
		{
			base.Unloading += MyAssemblyLoadContext_Unloading;
		}

		private void MyAssemblyLoadContext_Unloading(AssemblyLoadContext obj)
		{
			Console.WriteLine("当前正在卸载程序集:{0}", string.Join(", ", obj.Assemblies.Select((Assembly x) => x.GetName().Name)));
		}

		protected override Assembly? Load(AssemblyName assemblyName)
		{
			AssemblyName assemblyName2 = assemblyName;
			if (LoadFromDefault.Contains<string>(assemblyName2.Name))
			{
				Console.WriteLine("LoadFromDefault: {0} Version={1}", assemblyName2.Name, assemblyName2.Version);
				return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName2);
			}
			Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName2);
			if (!string.IsNullOrEmpty(assembly.Location))
			{
				Console.WriteLine("LoadFromAssemblyPath: {0} Version={1}", assemblyName2.Name, assemblyName2.Version);
				return LoadFromAssemblyPath(assembly.Location);
			}
			return base.Assemblies.ToList().Find((Assembly x) => x.GetName() == assemblyName2);
		}
        public void ReLoadDebugPlugin(string directoryPath)
        {
            TestPlugins.ForEach(delegate (TerrariaPlugin x)
            {
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 4);
                defaultInterpolatedStringHandler.AppendLiteral("[");
                defaultInterpolatedStringHandler.AppendFormatted(base.Name);
                defaultInterpolatedStringHandler.AppendLiteral("]Info: Plugin ");
                defaultInterpolatedStringHandler.AppendFormatted(x.Name);
                defaultInterpolatedStringHandler.AppendLiteral(" v");
                defaultInterpolatedStringHandler.AppendFormatted(x.Version);
                defaultInterpolatedStringHandler.AppendLiteral(" (by ");
                defaultInterpolatedStringHandler.AppendFormatted(x.Author);
                defaultInterpolatedStringHandler.AppendLiteral(") disponsed");
                Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
                x.Dispose();
            });
            Plugins.Clear();
            Unload();
        List<string> files = Directory.GetFiles(directoryPath, "*.dll").ToList();
			files.RemoveAll(x => x.Replace(directoryPath,"").Contains("TShcokAPI") || x.Replace(directoryPath, "").Contains("OTAPI") || x.Replace(directoryPath, "").Contains("TerrariaServer") || x.Replace(directoryPath, "").Contains("Newtonsoft.Json") || x.Replace(directoryPath, "").Contains("System.") || x.Replace(directoryPath, "").Contains("MonoMod.RuntimeDetour"));
            foreach (string path in files)
            {
                using FileStream assembly = File.OpenRead(path);
                using FileStream assemblySymbols = (File.Exists(Path.GetFileNameWithoutExtension(path) + ".pdb") ? File.OpenRead(Path.GetFileNameWithoutExtension(path) + ".pdb") : null);
                Type[] exportedTypes = LoadFromStream(assembly, assemblySymbols).GetExportedTypes();
                foreach (Type type in exportedTypes)
                {
                    if (type.IsSubclassOf(typeof(TerrariaPlugin)) && !type.IsAbstract)
                    {
                        TerrariaPlugin terrariaPlugin = (TerrariaPlugin)Activator.CreateInstance(type, Main.instance);
                        terrariaPlugin.Initialize();
                        TestPlugins.Add(terrariaPlugin);
                        DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 4);
                        defaultInterpolatedStringHandler.AppendLiteral("[");
                        defaultInterpolatedStringHandler.AppendFormatted(base.Name);
                        defaultInterpolatedStringHandler.AppendLiteral("]Info: Plugin ");
                        defaultInterpolatedStringHandler.AppendFormatted(terrariaPlugin.Name);
                        defaultInterpolatedStringHandler.AppendLiteral(" v");
                        defaultInterpolatedStringHandler.AppendFormatted(terrariaPlugin.Version);
                        defaultInterpolatedStringHandler.AppendLiteral(" (by ");
                        defaultInterpolatedStringHandler.AppendFormatted(terrariaPlugin.Author);
                        defaultInterpolatedStringHandler.AppendLiteral(") initiated");
						Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
                    }
                }
            }
        }
        public void LoadPlugin(TSPlayer player)
		{
			string[] files = Directory.GetFiles(PluginLoader.PluginPath, "*.dll");
			foreach (string path in files)
			{
				using FileStream assembly = File.OpenRead(path);
				using FileStream assemblySymbols = (File.Exists(Path.GetFileNameWithoutExtension(path) + ".pdb") ? File.OpenRead(Path.GetFileNameWithoutExtension(path) + ".pdb") : null);
				Type[] exportedTypes = LoadFromStream(assembly, assemblySymbols).GetExportedTypes();
				foreach (Type type in exportedTypes)
				{
					if (type.IsSubclassOf(typeof(TerrariaPlugin)) && !type.IsAbstract)
					{
						TerrariaPlugin terrariaPlugin = (TerrariaPlugin)Activator.CreateInstance(type, Main.instance);
						terrariaPlugin.Initialize();
						Plugins.Add(terrariaPlugin);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 4);
						defaultInterpolatedStringHandler.AppendLiteral("[");
						defaultInterpolatedStringHandler.AppendFormatted(base.Name);
						defaultInterpolatedStringHandler.AppendLiteral("]Info: Plugin ");
						defaultInterpolatedStringHandler.AppendFormatted(terrariaPlugin.Name);
						defaultInterpolatedStringHandler.AppendLiteral(" v");
						defaultInterpolatedStringHandler.AppendFormatted(terrariaPlugin.Version);
						defaultInterpolatedStringHandler.AppendLiteral(" (by ");
						defaultInterpolatedStringHandler.AppendFormatted(terrariaPlugin.Author);
						defaultInterpolatedStringHandler.AppendLiteral(") initiated");
						player.SendInfoMessage(defaultInterpolatedStringHandler.ToStringAndClear());
					}
				}
			}
		}

		public void UnloadPlugin(TSPlayer? player = null)
		{
			TSPlayer player2 = player;
			Plugins.ForEach(delegate(TerrariaPlugin x)
			{
				if (player2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 4);
					defaultInterpolatedStringHandler.AppendLiteral("[");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral("]Info]Info: Plugin ");
					defaultInterpolatedStringHandler.AppendFormatted(x.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" v");
					defaultInterpolatedStringHandler.AppendFormatted(x.Version);
					defaultInterpolatedStringHandler.AppendLiteral(" (by ");
					defaultInterpolatedStringHandler.AppendFormatted(x.Author);
					defaultInterpolatedStringHandler.AppendLiteral(") disponsed");
					Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					TSPlayer tSPlayer = player2;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 4);
					defaultInterpolatedStringHandler.AppendLiteral("[");
					defaultInterpolatedStringHandler.AppendFormatted(base.Name);
					defaultInterpolatedStringHandler.AppendLiteral("]Info: Plugin ");
					defaultInterpolatedStringHandler.AppendFormatted(x.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" v");
					defaultInterpolatedStringHandler.AppendFormatted(x.Version);
					defaultInterpolatedStringHandler.AppendLiteral(" (by ");
					defaultInterpolatedStringHandler.AppendFormatted(x.Author);
					defaultInterpolatedStringHandler.AppendLiteral(") disponsed");
					tSPlayer.SendInfoMessage(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				x.Dispose();
			});
			Plugins.Clear();
			Unload();
		}
	}
}
