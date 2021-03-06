using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using UnhollowerRuntimeLib;
using HarmonyLib;
using Object = UnityEngine.Object;


namespace TrackCrafts;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("xyz.molenzwiebel.wetstone")]
[Wetstone.API.Reloadable]
public class Plugin : BasePlugin
{
    /// <summary>
    ///     Most of the variables we need for the plugin.
    /// </summary>
    public static ManualLogSource Logger;

    public static ConfigEntry<bool> Enabled;
    private Harmony _harmony;
    public override void Load()
    {
        Logger = Log;
        _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
        Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        ClassInjector.RegisterTypeInIl2Cpp<TrackCrafts>();
        AddComponent<TrackCrafts>();
    }
    public override bool Unload()
    {
        Logger = Log;
        TrackCrafts.Reset();
        Object.Destroy(TrackCrafts.Instance);
        _harmony.UnpatchSelf();
        Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is unloaded!");
        return true;
    }

}
