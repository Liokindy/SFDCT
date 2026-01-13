using HarmonyLib;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Misc;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading;

namespace SFDCT;

/// <summary>
///     Entry point of SFDCT.
/// </summary>
internal static class Program
{
    internal static readonly Harmony Harmony = new("github.com/Liokindy/SFDCT");
    internal static readonly string GameDirectory = Path.GetFullPath(Directory.GetCurrentDirectory());

    private const string GitHubRepositoryVersionFileURL = "https://raw.githubusercontent.com/Liokindy/SFDCT/master/version";
    private const string GitHubRepositoryReleaseArchiveFileURL = "https://github.com/Liokindy/SFDCT/releases/download/VERSION/SFDCT.zip";
    private static WebClient UpdateWebClient = null;

    private static int Main(string[] args)
    {
        Logger.LogInfo("Thanks for downloading my mod! -Liokindy");
        Logger.LogInfo("Official repository: https://github.com/Liokindy/SFDCT");

#if DEBUG
        Logger.LogDebug("--------------------------------");
        Logger.LogDebug("RUNNING IN DEBUG MODE");
        Logger.LogDebug("DEBUG MESSAGES WILL APPEAR");
        Logger.LogDebug("--------------------------------");
#endif

        bool skipUpdateCheck = false;
        bool skipProgramChoice = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg.Equals("-SFD", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogInfo("Starting SFD...");

                Process.Start(Path.Combine(GameDirectory, "Superfighters Deluxe.exe"), string.Join(" ", args));
                return 0;
            }
            else if (arg.Equals("-HELP", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarn("Launch parameters");
                Logger.LogWarn("- HELP            Show this help message");
                Logger.LogWarn("- SKIP            Skip check version");
                Logger.LogWarn("- SERVER          Start SFDCT Dedicated Server");
                Logger.LogWarn("- SFD             Start vanilla SFD");
                return 0;
            }
            else if (arg.Equals("-SKIP", StringComparison.OrdinalIgnoreCase))
            {
                skipUpdateCheck = true;
                skipProgramChoice = true;
            }
        }
        
        if (!skipProgramChoice)
        {
            Logger.LogWarn("0. SFDCT; 1. SFD");
            Logger.LogWarn("Start option: ", false);

            ConsoleKeyInfo k = new();
            for (int cnt = 0; cnt < 6; cnt++)
            {
                if (Console.KeyAvailable)
                {
                    k = Console.ReadKey();
                    break;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }

            Console.WriteLine();

            if (k.Key == ConsoleKey.D1 || k.Key == ConsoleKey.NumPad1)
            {
                Logger.LogInfo("Starting SFD");

                string SFD_exe = Path.Combine(GameDirectory, "Superfighters Deluxe.exe");
                Process.Start(SFD_exe, string.Join(" ", args));
                return 0;
            }
        }

        if (!skipUpdateCheck)
        {
            Logger.LogWarn("Checking for updates from repository...");
            Logger.LogWarn("To skip version fetching, use -skip");

            if (CheckForUpdates())
            {
                Logger.LogWarn("Disposing WebClient");
                UpdateWebClient?.Dispose();
                UpdateWebClient = null;

                Logger.LogInfo("Restart SFDCT to finish update");
                return 0;
            }

            Logger.LogWarn("Disposing WebClient");
            UpdateWebClient?.Dispose();
            UpdateWebClient = null;
        }

        Logger.LogInfo($"Starting SFDCT {Globals.Version.SFDCT} for SFD {Globals.Version.SFD}");

        Logger.LogInfo("- Loading Configuration");
        SFDCTConfig.LoadFile();

        Stopwatch patchStopWatch = new();
        patchStopWatch.Start();

        Logger.LogInfo("- Loading Harmony");
        Harmony.PatchAll();

        Logger.LogInfo($"Starting ({patchStopWatch.ElapsedMilliseconds}ms)");

        patchStopWatch.Stop();
        patchStopWatch = null;

        SFD.Program.Main(args);

        return 0;
    }

    private static bool CheckForUpdates()
    {
        UpdateWebClient = new();

        string repositoryVersion;

        try
        {
            Logger.LogWarn("Fetching version");
            repositoryVersion = UpdateWebClient.DownloadString(GitHubRepositoryVersionFileURL).Trim();
        }
        catch (WebException ex)
        {
            Logger.LogError("Error fetching version from repository:");
            Logger.LogError(ex.Message);
            return false;
        }

        string updateUrl = GitHubRepositoryReleaseArchiveFileURL.Replace("VERSION", repositoryVersion);

        Logger.LogWarn($"- Current: {Globals.Version.SFDCT}, Repository: {repositoryVersion}");

        switch (string.CompareOrdinal(Globals.Version.SFDCT, repositoryVersion))
        {
            case >= 0:
                Logger.LogWarn("Current version is equal or newer than repository");
                return false;
            case < 0:
                Logger.LogWarn("Current version is older than repository");
                return DownloadUpdate();
        }
    }

    private static bool DownloadUpdate()
    {
        Logger.LogWarn($"- Files at '{Globals.Paths.SFDCT}' will be deleted.");
        Logger.LogWarn($"- Files at '{Globals.Paths.SubContent}' will be kept.");
        Logger.LogWarn($"- Configuration file at '{Globals.Paths.ConfigurationIni}' will be kept.");
        Logger.LogWarn("Download Update? (Y/N): ", false);
        bool choice = (Console.ReadLine() ?? string.Empty).Equals("Y", StringComparison.OrdinalIgnoreCase);

        if (!choice) return false;

        string archivePath = Path.Combine(GameDirectory, "SFDCT.zip");
        Logger.LogWarn("Downloading update archive");

        try
        {
            UpdateWebClient.DownloadFile(GitHubRepositoryReleaseArchiveFileURL, archivePath);
        }
        catch
        {
            return false;
        }

        Logger.LogWarn("Replacing files");

        ReplaceOldFile(Assembly.GetExecutingAssembly().Location); // SFDCT.exe
        ReplaceOldFile(Path.Combine(GameDirectory, "SFDCT.exe.config")); // SFDCT.exe.config

        // SFDCT/*.dll
        foreach (string file in Directory.GetFiles(Globals.Paths.SFDCT, "*.dll", SearchOption.TopDirectoryOnly))
        {
            ReplaceOldFile(file);
        }

        // SFDCT/Content/*
        Logger.LogWarn("Deleting old content files");
        foreach (string file in Directory.GetFiles(Path.Combine(Globals.Paths.Content), "*.*", SearchOption.AllDirectories))
        {
            File.Delete(file);
        }

        Logger.LogWarn("Extracting update archive");
        using (ZipArchive archive = ZipFile.OpenRead(archivePath))
        {
            archive.ExtractToDirectory(GameDirectory);
        }

        Logger.LogWarn("Deleting update archive");
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