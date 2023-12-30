using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SFD.Code;
using SFR.Helper;
using SFR.Fighter;
using SFD;
using System.Threading;

namespace SFR.Misc;

internal static class ConfigIni
{
    private static IniHandler Handler;
    /*
        IniHandler inihand = new IniHandler();
        if (!File.Exists(Misc.Constants.Paths.ConfigurationIni))
        {
            using (FileStream fileStream = File.Create(Misc.Constants.Paths.ConfigurationIni))
            {
                fileStream.Close();
            }
            Thread.Sleep(50);

            inihand.ReadLine("A_FLOAT=1");
            inihand.SaveFile(Misc.Constants.Paths.ConfigurationIni);
        }
        inihand.ReadFile(Misc.Constants.Paths.ConfigurationIni);
        float a_float = inihand.ReadValueFloat("A_FLOAT", -1f);
    */

    /// <summary>
    ///     Initializes the config.ini, if it doesnt exist 
    ///     it will be created and assigned default values.
    /// </summary>
    public static void Initialize()
    {
        Handler = new IniHandler();

        // Create config.ini if it doesnt exist.
        if (!File.Exists(Constants.Paths.ConfigurationIni))
        {
            using (FileStream fileStream = File.Create(Misc.Constants.Paths.ConfigurationIni))
            {
                fileStream.Close();
            }
            Thread.Sleep(50);

            // Write default values to it
            SetDefaultValues();
            Handler.SaveFile(Constants.Paths.ConfigurationIni);
        }

        Handler.ReadFile(Constants.Paths.ConfigurationIni);
        SetCurrentValues();
    }

    /// <summary>
    ///     Loads the config.ini again and refreshes the settings
    ///     if they changed outside the game.
    /// </summary>
    public static void Refresh()
    {
        ConsoleOutput.ShowMessage(ConsoleOutputType.Information, "Refreshing config.ini...");
        if (!File.Exists(Constants.Paths.ConfigurationIni))
        {
            Logger.LogError("config.ini: Cannot refresh - file doesnt exist. Restart the game to create it again");
            return;
        }
        if (Handler == null || Handler.IsDisposed)
        {
            Logger.LogError("config.ini: Cannot refresh - handler is null or disposed");
            return;
        }

        Handler.Clear();
        Handler.ReadFile(Constants.Paths.ConfigurationIni);
        SetCurrentValues();
    }

    private static void ReadLine(string key, string value)
    {
        Handler.ReadLine(key + "=" + value);
    }

    private static void SetCurrentValues()
    {

        Constants.SoundPanning_Strength = Handler.ReadValueFloat(nameof(Constants.SoundPanning_Strength), Constants.SoundPanning_Strength);
        Constants.SoundPanning_IsScreenSpace = Handler.ReadValueBool(nameof(Constants.SoundPanning_IsScreenSpace), Constants.SoundPanning_IsScreenSpace);
        Constants.SoundPanning_InWorld_Threshold = Handler.ReadValueFloat(nameof(Constants.SoundPanning_InWorld_Threshold), Constants.SoundPanning_InWorld_Threshold);
        Constants.SoundPanning_InWorld_Distance = Handler.ReadValueFloat(nameof(Constants.SoundPanning_InWorld_Distance), Constants.SoundPanning_InWorld_Distance);

        Microsoft.Xna.Framework.Color MenuColor;
        if (Handler.TryReadValueColor("MenuColor", SFD.Constants.COLORS.MENU_BLUE, out MenuColor))
        {
            MenuColor.A = 255;
            SFD.Constants.COLORS.MENU_BLUE = MenuColor;
        }


    }
    private static void SetDefaultValues()
    {

        ReadLine(nameof(Constants.SoundPanning_Strength), Constants.SoundPanning_Strength.ToString());
        ReadLine(nameof(Constants.SoundPanning_IsScreenSpace), Constants.SoundPanning_IsScreenSpace.ToString());
        ReadLine(nameof(Constants.SoundPanning_InWorld_Threshold), Constants.SoundPanning_InWorld_Threshold.ToString());
        ReadLine(nameof(Constants.SoundPanning_InWorld_Distance), Constants.SoundPanning_InWorld_Distance.ToString());

        ReadLine("MenuColor", SFD.Constants.ColorToString(SFD.Constants.COLORS.MENU_BLUE));
    }
}