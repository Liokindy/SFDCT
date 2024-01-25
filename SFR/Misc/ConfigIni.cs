using System;
using System.Collections.Generic;
using System.IO;
using SFD.Code;
using SFDCT.Helper;
using System.Threading;
using CSettings = SFDCT.Settings.Values;
using HarmonyLib;

namespace SFDCT.Misc;

[HarmonyPatch]
internal static class RefreshIni
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.ConsoleOutput), nameof(SFD.ConsoleOutput.Show))]
    private static void RefreshIniOnConsoleOutputShow()
    {
        ConfigIni.Refresh();
    }
}
internal static class ConfigIni
{
    private static IniHandler Handler;

    /// <summary>
    ///     Initializes the config.ini, if it doesnt exist 
    ///     it will be created and assigned default values.
    /// </summary>
    public static void Initialize()
    {
        Handler = new IniHandler();

        // Initialize settings list
        CSettings.Init();

        // Create config.ini if it doesnt exist.
        if (!File.Exists(Constants.Paths.ConfigurationIni))
        {
            using (FileStream fileStream = File.Create(Misc.Constants.Paths.ConfigurationIni))
            {
                fileStream.Close();
            }
            Thread.Sleep(100);

            Handler.ReadLine(";Remember that floats are written with ',' instead of '.' i.e: Value=0,75");
            Handler.ReadLine(";If setting order seems chaotic or shuffled randomly you might want to make a copy of this 'config.ini', rename it and let the game create a new 'config.ini'. Then manually copy your custom settings to it.");
            // Write default values to it
            foreach (KeyValuePair<string, CSettings.IniSetting> kvp in CSettings.List)
            {
                kvp.Value.Save(Handler);
            }

            Handler.SaveFile(Constants.Paths.ConfigurationIni);
            Handler.Clear();
        }
        Refresh();
    }
    public static void Refresh()
    {
        Logger.LogDebug("CONFIG.INI: Refreshing...");
        if (!File.Exists(Constants.Paths.ConfigurationIni))
        {
            Logger.LogError("CONFIG.INI: File doesnt exist. Restart the game to create it again");
            return;
        }
        if (Handler == null || Handler.IsDisposed)
        {
            Logger.LogError("CONFIG.INI: Handler is null or disposed");
            return;
        }

        Handler.Clear();
        Handler.ReadFile(Constants.Paths.ConfigurationIni);
        foreach (KeyValuePair<string, CSettings.IniSetting> kvp in CSettings.List)
        {
            kvp.Value.Load(Handler);
        }
        Handler.SaveFile(Constants.Paths.ConfigurationIni);
        CSettings.ApplyOverrides();
    }
}