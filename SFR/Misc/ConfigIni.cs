using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SFD.Code;
using SFR.Helper;
using SFR.Fighter;
using SFD;
using System.Threading;
using System.Windows.Forms;
using SFRSettings = SFR.Settings.Values;

namespace SFR.Misc;

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
        SFRSettings.Init();

        // Create config.ini if it doesnt exist.
        if (!File.Exists(Constants.Paths.ConfigurationIni))
        {
            using (FileStream fileStream = File.Create(Misc.Constants.Paths.ConfigurationIni))
            {
                fileStream.Close();
            }
            Thread.Sleep(50);

            Handler.ReadLine(";Liokindy was here");
            // Write default values to it
            foreach (KeyValuePair<string, SFRSettings.IniSetting> kvp in SFRSettings.List)
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
        foreach (KeyValuePair<string, SFRSettings.IniSetting> kvp in SFRSettings.List)
        {
            kvp.Value.Load(Handler);
        }
        SFRSettings.ApplyOverrides();
    }
}