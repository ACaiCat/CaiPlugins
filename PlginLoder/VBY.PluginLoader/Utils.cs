using System;
using System.Collections;
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;

namespace VBY.PluginLoader
{
	public static class Utils
	{
		public static void ClearOwner(Delegate hook)
		{
			IDictionary dictionary = (IDictionary)typeof(HookEndpointManager).GetField("OwnedHookLists", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null);
			object owner = HookEndpointManager.GetOwner(hook);
			if (owner != null)
			{
				dictionary.Remove(owner);
			}
		}
	}
}
