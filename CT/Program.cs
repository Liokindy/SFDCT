using HarmonyLib;
using SFDCT.Helper;
//using SFDCT.Misc;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SFDCT;

/// <summary>
///     Entry point of SFDCT. This class will decide what game to start, SFDCT, vanilla-SFD, or SFR (if available).
/// </summary>
internal static class Program
{
    internal static readonly string GameDirectory = Directory.GetCurrentDirectory();
    internal static readonly string GitHubRepositoryURL = "https://github.com/Liokindy/SFDCT";
    internal static readonly string GitHubVersionFileURL = "https://raw.githubusercontent.com/Liokindy/SFDCT/master/ModVersion.txt";
    private static readonly Harmony Harmony = new("SuperfightersDeluxe_Custom");

    private static int Main(string[] args)
    {
        // Patch SFD and start SFDCT
        Logger.LogInfo("Starting SFDCT...");
        Harmony.PatchAll();
        Logger.LogInfo("Patching completed...");
        SFD.Program.Main(args);

        return 0;
    }
}