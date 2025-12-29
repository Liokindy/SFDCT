using Microsoft.Xna.Framework;
using SFD;
using SFD.Colors;
using SFD.Parser;
using SFDCT.Helper;
using System.Collections.Generic;
using System.IO;

namespace SFDCT.Assets;

internal static class ColorLoader
{
    internal static void ReadColorFile(string filePath)
    {
        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading colors from file '{filePath}'");
        SFDXReader.ReadDataFromSFDXFile(filePath);
    }

    internal static void Load()
    {
        Dictionary<string, Color[]> colorsDic = [];

        Logger.LogInfo($"LOADING [COLORS]: Official");
        string officialColorsPath = Path.Combine(Constants.Paths.ContentPath, Constants.Paths.DATA_COLORS_COLORS);
        foreach (var paletteFile in Directory.GetFiles(officialColorsPath, "*.sfdx"))
        {
            ReadColorFile(paletteFile);
        }

        SubContentHandler.CopyAndReplaceDictionary(ColorDatabase.m_colors, colorsDic);
        ColorDatabase.m_colors.Clear();

        string documentsColorsPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_COLORS_COLORS);
        if (Directory.Exists(documentsColorsPath))
        {
            Logger.LogInfo($"LOADING [COLORS]: Documents");
            foreach (var paletteFile in Directory.GetFiles(documentsColorsPath, "*.sfdx"))
            {
                ReadColorFile(paletteFile);
            }

            SubContentHandler.CopyAndReplaceDictionary(ColorDatabase.m_colors, colorsDic);
            ColorDatabase.m_colors.Clear();
        }

        foreach (var subContentFolder in SubContentHandler.Folders)
        {
            string subContentColorsPath = SubContentHandler.GetPath(subContentFolder, Constants.Paths.DATA_COLORS_COLORS);
            if (Directory.Exists(subContentColorsPath))
            {
                Logger.LogInfo($"LOADING [COLORS]: {subContentFolder}");

                foreach (var paletteFile in Directory.GetFiles(subContentColorsPath, "*.sfdx"))
                {
                    ReadColorFile(paletteFile);
                }

                SubContentHandler.CopyAndReplaceDictionary(ColorDatabase.m_colors, colorsDic);
                ColorDatabase.m_colors.Clear();
            }
        }

        ColorDatabase.m_colors = colorsDic;
    }
}
