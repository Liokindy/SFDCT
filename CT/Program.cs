using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SFDCT.Helper;
using SFDCT.Misc;
using SFDCT.Configuration;
using HarmonyLib;

namespace SFDCT;

/// <summary>
///     Entry point of SFDCT.
/// </summary>
internal static class Program
{
    internal static readonly string GameDirectory = Directory.GetCurrentDirectory();
    
    internal static readonly string GitHubRepositoryURL = "https://github.com/Liokindy/SFDCT";
    
    internal static bool DebugMode = false;
    private static readonly Harmony Harmony = new("SuperfightersDeluxe_Custom");

    private static int Main(string[] args)
    {
        if (args.Contains("-SFD", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogInfo("Initializing SFD...");
            SFD.Program.Main(args);

            return 0;
        }

        if (args.Contains("-DEBUG", StringComparer.OrdinalIgnoreCase))
        {
            DebugMode = true;
        }

#if DEBUG
        DebugMode = true;
#endif

        if (DebugMode)
        {
            Logger.LogWarn("--------------------------------");
            Logger.LogWarn("RUNNING IN DEBUG MODE");
            Logger.LogWarn("DEBUG MESSAGES WILL APPEAR");
            Logger.LogWarn("--------------------------------");
        }

        Stopwatch sw = new();
        sw.Start();

        Logger.LogInfo("Initializing SFDCT...");
        IniFile.Initialize();

        Logger.LogInfo("Patching SFD...");
        Harmony.PatchAll();

        sw.Stop();
        Logger.LogInfo($"Initialized and Patched in {sw.ElapsedMilliseconds}ms");
        sw = null;

        Logger.LogInfo("Starting SFD...");
        SFD.Program.Main(args);

        return 0;
    }
}