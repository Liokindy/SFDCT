using System.Collections.Generic;
using System.IO;
using System.Threading;
using SFD.Code;
using SFDCT.Misc;
using SFDCT.Helper;
using HarmonyLib;

namespace SFDCT.Configuration;

[HarmonyPatch]
internal static class Refresh
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.GameSFD), nameof(SFD.GameSFD.StateKeyUpEvent))]
    private static void RefreshIniOnKeyUp(Microsoft.Xna.Framework.Input.Keys key)
    {
        if (key == Microsoft.Xna.Framework.Input.Keys.F6)
        {
            SFD.ConsoleOutput.ShowMessage(SFD.ConsoleOutputType.ScriptFiles, $"Refreshing '{Globals.Paths.CONFIGURATIONINI}'...");
            IniFile.Refresh();
            SFD.ConsoleOutput.ShowMessage(SFD.ConsoleOutputType.ScriptFiles, "Refreshed!");
        }
    }
}

internal static class IniFile
{
    public static IniHandler Handler;
    public static bool NeedsSaving = false;
    public static bool FirstRefresh = true;

    /// <summary>
    ///     Initializes the config.ini, if it doesnt exist 
    ///     it will be created and assigned default values.
    /// </summary>
    public static void Initialize()
    {
        Handler = new IniHandler();

        // Initialize settings list
        Settings.Init();

        // Create config.ini if it doesnt exist.
        if (!System.IO.File.Exists(Globals.Paths.CONFIGURATIONINI))
        {
            using (FileStream fileStream = System.IO.File.Create(Globals.Paths.CONFIGURATIONINI))
            {
                fileStream.Close();
            }
            Thread.Sleep(100);

            Handler.ReadLine(";Floats are saved with '.' i.e: VALUE=0.65");
            Handler.ReadLine(";If setting order seems chaotic or shuffled randomly you might want to make a copy of this 'config.ini', rename it and let the game create a new 'config.ini'. Then manually copy your custom settings to it.");

            // Write default values to it
            foreach (KeyValuePair<string, IniSetting> kvp in Settings.List)
            {
                kvp.Value.Save(Handler);
            }

            Handler.SaveFile(Globals.Paths.CONFIGURATIONINI);
            Handler.Clear();
        }
        Refresh();
    }
    public static void Refresh()
    {
        Logger.LogDebug("CONFIG.INI: Refreshing...");
        if (!System.IO.File.Exists(Globals.Paths.CONFIGURATIONINI))
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
        Handler.ReadFile(Globals.Paths.CONFIGURATIONINI);
        foreach (KeyValuePair<string, IniSetting> kvp in Settings.List)
        {
            kvp.Value.Load(Handler);
        }
        Handler.SaveFile(Globals.Paths.CONFIGURATIONINI);
        Settings.ApplyOverrides();

        if (FirstRefresh)
        {
            FirstRefresh = false;
        }
    }
    public static void Save()
    {
        if (!NeedsSaving)
        {
            return;
        }

        Logger.LogDebug("CONFIG.INI: Saving...");
        if (!System.IO.File.Exists(Globals.Paths.CONFIGURATIONINI))
        {
            Logger.LogError("CONFIG.INI: Cannot save, file doesnt exist.");
            return;
        }
        if (Handler == null || Handler.IsDisposed)
        {
            Logger.LogError("CONFIG.INI: Handler is null or disposed");
            return;
        }

        foreach (KeyValuePair<string, IniSetting> kvp in Settings.List)
        {
            kvp.Value.Save(Handler);
        }
        Handler.SaveFile(Globals.Paths.CONFIGURATIONINI);
        Logger.LogDebug("CONFIG.INI: Saving finished.");
    }
}