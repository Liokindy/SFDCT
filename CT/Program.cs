using HarmonyLib;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Misc;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;

namespace SFDCT;

/// <summary>
///     Entry point of SFDCT.
/// </summary>
internal static class Program
{
    internal static readonly string GameDirectory = Path.GetFullPath(Directory.GetCurrentDirectory());
    internal static readonly string GitHubRepositoryURL = "https://github.com/Liokindy/SFDCT";
    internal static readonly string GitHubRepositoryVersionFileURL = "https://raw.githubusercontent.com/Liokindy/SFDCT/master/version";

    internal static readonly Harmony Harmony = new("github.com/Liokindy/SFDCT");
    internal static bool DebugMode = false;

    private static WebClient updateWebClient = null;
    private static string updateUrl = "https://github.com/Liokindy/SFDCT/releases/download/VERSION/SFDCT.zip";

    private static int Main(string[] args)
    {
        foreach (string file in Directory.GetFiles(GameDirectory, "*.old", SearchOption.TopDirectoryOnly))
        {
            File.Delete(file);
        }

        foreach (string file in Directory.GetFiles(Globals.Paths.SFDCT, "*.old", SearchOption.TopDirectoryOnly))
        {
            File.Delete(file);
        }

        if (args.Contains("-HELP", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogInfo("Launch parameters");
            Logger.LogWarn("- HELP            Show this help message");
            Logger.LogWarn("- SKIP            Skip version fetching");
            Logger.LogWarn("- SFD             Start SFD");
            Logger.LogWarn("- DEBUG           Run in debug mode, shows debug messages");
            return 0;
        }

        if (args.Contains("-SFD", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogInfo("Starting SFD...");

            string sfdFile = Path.Combine(GameDirectory, "Superfighters Deluxe.exe");
            if (File.Exists(sfdFile))
            {
                Process.Start(sfdFile, string.Join(" ", args));
                return 0;
            }

            Logger.LogError("SFD file not found");
            return -1;
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
            Logger.LogDebug("--------------------------------");
            Logger.LogDebug("RUNNING IN DEBUG MODE");
            Logger.LogDebug("DEBUG MESSAGES WILL APPEAR");
            Logger.LogDebug("--------------------------------");
        }

        Stopwatch sw = new();
        sw.Start();

        if (!args.Contains("SKIP", StringComparer.OrdinalIgnoreCase))
        {
            Logger.LogWarn("Checking for updates from repository...");
            Logger.LogWarn("To skip version fetching, use -skip");

            if (CheckForUpdates())
            {
                Logger.LogWarn("Disposing web client");
                Logger.LogWarn("Restart SFDCT to use newest repository version");

                updateWebClient?.Dispose();
                updateWebClient = null;
                return 0;
            }

            Logger.LogWarn("Disposing web client");
            updateWebClient?.Dispose();
            updateWebClient = null;
        }

        Logger.LogInfo("Current SFDCT version is: " + Globals.Version.SFDCT);
        Logger.LogInfo("Target SFD version is: " + Globals.Version.SFD);

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

    private static bool CheckForUpdates()
    {
        updateWebClient = new();

        string repositoryVersion;
        Logger.LogWarn("- Fetching version from repository...");

        try
        {
            repositoryVersion = updateWebClient.DownloadString(GitHubRepositoryVersionFileURL).Trim();
        }
        catch (WebException)
        {
            return false;
        }

        updateUrl = updateUrl.Replace("VERSION", repositoryVersion);

        Logger.LogWarn($"- Current: {Globals.Version.SFDCT}");
        Logger.LogWarn($"- Repository: {repositoryVersion}");

        switch (string.CompareOrdinal(Globals.Version.SFDCT, repositoryVersion))
        {
            case >= 0:
                Logger.LogWarn("Current version is the same or newer than repository");
                return false;
            case < 0:
                return DownloadUpdate();
        }
    }

    private static bool DownloadUpdate()
    {
        Logger.LogWarn($"All files at '{Globals.Paths.SFDCT}' will be deleted permanently");
        Logger.LogWarn("Proceed? (Y/N): ", false);
        bool choice = (Console.ReadLine() ?? string.Empty).Equals("Y", StringComparison.OrdinalIgnoreCase);

        if (!choice) return false;

        string archivePath = Path.Combine(GameDirectory, "SFDCT.zip");
        Logger.LogWarn("Downloading repository version...");

        try
        {
            updateWebClient.DownloadFile(updateUrl, archivePath);
        }
        catch (Exception)
        {
            return false;
        }

        Logger.LogWarn("- Replacing current files...");

        ReplaceOldFile(Assembly.GetExecutingAssembly().Location);
        ReplaceOldFile(Path.Combine(GameDirectory, "SFDCT.exe.config"));

        foreach (string file in Directory.GetFiles(Globals.Paths.SFDCT, "*.dll", SearchOption.TopDirectoryOnly))
        {
            ReplaceOldFile(file);
        }

        foreach (string file in Directory.GetFiles(Path.Combine(Globals.Paths.SFDCT, "Content"), "*.*", SearchOption.AllDirectories))
        {
            File.Delete(file);
        }

        Logger.LogWarn("- Extracting repository version...");

        using (ZipArchive archive = ZipFile.OpenRead(archivePath))
        {
            archive.ExtractToDirectory(GameDirectory);
        }

        File.Delete(archivePath);
        return true;
    }

    private static void ReplaceOldFile(string file)
    {
        string newExtension = Path.ChangeExtension(file, "old");
        if (File.Exists(newExtension)) File.Delete(newExtension);

        File.Move(file, newExtension);
    }
}