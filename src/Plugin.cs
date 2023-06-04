using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Object = UnityEngine.Object;
namespace TrackCrafts;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("gg.deca.Bloodstone")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin
{
    private const string PluginGuid = "armanckeser.vrising.trackcrafts";
    private const string PluginName = "TrackCrafts";
    private const string PluginVersion = "0.0.6";
     public static ManualLogSource Logger { get; private set; }
    public static ConfigEntry<int> TrackQuantity { get; private set; }
    public static ConfigEntry<float> TrackerItemScale { get; private set; }
    private Harmony _harmony;
    public override void Load()
    {
        TrackerItemScale = Config.Bind("General", "TrackerItemScale", 0.75f, new ConfigDescription("Scale of the tracker item", new AcceptableValueRange<float>(0.1f, 2f)));
        TrackQuantity = Config.Bind("General", "TrackQuantity", 3, "Number of items to track");
        Logger = Log;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
        Log.LogInfo($"Plugin {PluginGuid} is loaded!");
        ClassInjector.RegisterTypeInIl2Cpp<TrackCrafts>();
        AddComponent<TrackCrafts>();
    }
    public override bool Unload()
    {
        //Reset is a Unity even function, hence renaming to ResetAll
        TrackCrafts.ResetAll();
        Object.Destroy(TrackCrafts.Instance);
        _harmony.UnpatchSelf();
        Log.LogInfo($"Plugin {PluginGuid} is unloaded!");
        return true;
    }

}