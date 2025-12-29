using SFD;
using SFD.Colors;
using SFD.Parser;
using SFDCT.Helper;
using System.Collections.Generic;
using System.IO;

namespace SFDCT.Assets;

internal static class ColorPaletteLoader
{
    internal static void ReadColorPaletteFile(string filePath)
    {
        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading color palettes from file '{filePath}'");
        SFDXReader.ReadDataFromSFDXFile(filePath);
    }

    internal static void Load()
    {
        Dictionary<string, ColorPalette> colorPalettes = [];

        Logger.LogInfo("LOADING [COLOR PALETTES]: Official");
        string officialColorPalettesPath = Path.Combine(Constants.Paths.ContentPath, Constants.Paths.DATA_COLORS_PALETTES);
        foreach (string filePath in Directory.GetFiles(officialColorPalettesPath, "*.sfdx", SearchOption.AllDirectories))
        {
            ReadColorPaletteFile(filePath);
        }

        SubContentHandler.CopyAndReplaceDictionary(ColorPaletteDatabase.m_palettes, colorPalettes);
        ColorPaletteDatabase.m_palettes.Clear();

        string documentsColorPalettesPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_COLORS_PALETTES);
        if (Directory.Exists(documentsColorPalettesPath))
        {
            Logger.LogInfo("LOADING [COLOR PALETTES]: Documents");
            foreach (string filePath in Directory.GetFiles(documentsColorPalettesPath, "*.sfdx", SearchOption.AllDirectories))
            {
                ReadColorPaletteFile(filePath);
            }

            SubContentHandler.CopyAndReplaceDictionary(ColorPaletteDatabase.m_palettes, colorPalettes);
            ColorPaletteDatabase.m_palettes.Clear();
        }

        foreach (var subContentFolder in SubContentHandler.Folders)
        {
            string subContentColorPalettesPath = SubContentHandler.GetPath(subContentFolder, Constants.Paths.DATA_COLORS_PALETTES);

            if (Directory.Exists(subContentColorPalettesPath))
            {
                Logger.LogInfo($"LOADING [COLOR PALETTES]: {subContentFolder}");

                foreach (string filePath in Directory.GetFiles(subContentColorPalettesPath, "*.sfdx", SearchOption.AllDirectories))
                {
                    ReadColorPaletteFile(filePath);
                }

                SubContentHandler.CopyAndReplaceDictionary(ColorPaletteDatabase.m_palettes, colorPalettes);
                ColorPaletteDatabase.m_palettes.Clear();
            }
        }

        ColorPaletteDatabase.m_palettes = colorPalettes;
    }
}
