using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using System;
using System.IO;
using UnityEngine;

namespace FPSUnlocker;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class FPSUnlock : BasePlugin
{
    internal static new ManualLogSource Log;
    private static ConfigEntry<int> Framerate;
    private static ConfigEntry<vSyncList> vSync;
    private static vSyncList _originalVsync = vSyncList.Default;
    private static int lastFps;

    private FileSystemWatcher watcher;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Log.LogInfo($"Original Framerate: {Application.targetFrameRate}");
        Log.LogInfo($"Original vSync: {QualitySettings.vSyncCount}");

        vSync = Config.Bind("Framerate Override", "vSync", vSyncList.Default, "Force specified vsync mode.");
        Framerate = Config.Bind("Framerate Override", "Target Framerate", Application.targetFrameRate, "Force specified target Framerate. Only works if vSync is Off. Set -1 for unlimited.");

        lastFps = Framerate.Value;

        watcher = new FileSystemWatcher(Path.GetDirectoryName(Config.ConfigFilePath))
        {
            Filter = Path.GetFileName(Config.ConfigFilePath),
            NotifyFilter = NotifyFilters.LastWrite
        };
        watcher.Changed += OnConfigChanged;
        watcher.EnableRaisingEvents = true;
        Log.LogInfo($"Config Path: {Path.GetFileName(Config.ConfigFilePath)}");

        ApplySettings();
    }

    private static void ApplySettings()
    {
        QualitySettings.vSyncCount = (int)vSync.Value;
        Application.targetFrameRate = Framerate.Value;

        Log.LogInfo($"Current Framerate: {Application.targetFrameRate}");
        Log.LogInfo($"Current vSync: {QualitySettings.vSyncCount}");
    }

    private void OnConfigChanged(object sender, FileSystemEventArgs e)
    {
        Log.LogInfo($"Configuration changed!");
        try
        {
            Config.Reload();
            ApplySettings();
        }
        catch(Exception ex)
        {
            Log.LogError(ex.Message);
        }
    }

    private enum vSyncList
    {
        Default = -1,
        On = 1,
        Off = 0,
        Half = 2
    }
}
