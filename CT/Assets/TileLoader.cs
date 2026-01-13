using SFD;
using SFD.Parser;
using SFD.Tiles;
using SFDCT.Helper;
using System.Collections.Generic;
using System.IO;

namespace SFDCT.Assets;

internal static class TileLoader
{
    internal static void ReadTileFile(string filePath)
    {
        ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading tiles from file '{filePath}'");
        SFDXReader.ReadDataFromSFDXFile(filePath);
    }

    internal static void Load()
    {
        Dictionary<string, Tile> tiles = [];

        Logger.LogInfo("LOADING [TILES]: Official");
        string officialTilesPath = Path.Combine(Constants.Paths.ContentPath, Constants.Paths.DATA_TILES);
        foreach (var filePath in Directory.GetFiles(officialTilesPath, "*.sfdx"))
        {
            ReadTileFile(filePath);
        }

        SubContentHandler.CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
        TileDatabase.m_tiles.Clear();
        TileDatabase.m_categorizedTiles.Clear();

        string officialWeaponTilesPath = Path.Combine(Constants.Paths.ContentPath, Constants.Paths.DATA_WEAPONS);
        foreach (var filePath in Directory.GetFiles(officialWeaponTilesPath, "*.sfdx"))
        {
            ReadTileFile(filePath);
        }

        SubContentHandler.CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
        TileDatabase.m_tiles.Clear();
        TileDatabase.m_categorizedTiles.Clear();

        string documentsTilesPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_TILES);
        if (Directory.Exists(documentsTilesPath))
        {
            Logger.LogInfo("LOADING [TILES]: Documents");
            foreach (var filePath in Directory.GetFiles(documentsTilesPath, "*.sfdx"))
            {
                ReadTileFile(filePath);
            }

            SubContentHandler.CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
            TileDatabase.m_tiles.Clear();
            TileDatabase.m_categorizedTiles.Clear();
        }

        string documentsWeaponTilesPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_WEAPONS);
        if (Directory.Exists(documentsWeaponTilesPath))
        {
            foreach (var filePath in Directory.GetFiles(documentsWeaponTilesPath, "*.sfdx"))
            {
                ReadTileFile(filePath);
            }

            SubContentHandler.CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
            TileDatabase.m_tiles.Clear();
            TileDatabase.m_categorizedTiles.Clear();
        }

        foreach (var subContentFolder in SubContentHandler.Folders)
        {
            string subContentTilesPath = SubContentHandler.GetPath(subContentFolder, Constants.Paths.DATA_TILES);

            if (Directory.Exists(subContentTilesPath))
            {
                Logger.LogInfo($"LOADING [TILES]: {subContentFolder}");

                foreach (string filePath in Directory.GetFiles(subContentTilesPath, "*.sfdx"))
                {
                    ReadTileFile(filePath);
                }

                SubContentHandler.CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
                TileDatabase.m_tiles.Clear();
                TileDatabase.m_categorizedTiles.Clear();
            }

            string subContentWeaponTilesPath = SubContentHandler.GetPath(subContentFolder, Constants.Paths.DATA_WEAPONS);

            if (Directory.Exists(subContentWeaponTilesPath))
            {
                foreach (string filePath in Directory.GetFiles(subContentWeaponTilesPath, "*.sfdx"))
                {
                    ReadTileFile(filePath);
                }

                SubContentHandler.CopyAndReplaceDictionary(TileDatabase.m_tiles, tiles);
                TileDatabase.m_tiles.Clear();
                TileDatabase.m_categorizedTiles.Clear();
            }
        }

        foreach (var tile in tiles.Values)
        {
            TileDatabase.Add(tile);
        }

        TileDatabase.Add(new Tile(TileStructure.GetPlayerTileStructure()), true);
    }
}
