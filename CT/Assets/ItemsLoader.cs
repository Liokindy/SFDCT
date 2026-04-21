using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Code;
using SFD.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SFDCT.Assets;

internal static class ItemsLoader
{
    internal static Item ReadItemFromFolder(GameSFD game, string folderPath, string iniPath)
    {
        // properties
        var propertiesHandler = new IniHandler();
        propertiesHandler.ReadFile(iniPath);

        var itemParts = new List<ItemPart>();
        var itemGameName = propertiesHandler.ReadValue("GameName");
        var itemFileName = propertiesHandler.ReadValue("FileName");
        var itemEquipmentLayer = propertiesHandler.ReadValueInt("EquipmentLayer", 0);
        var itemID = propertiesHandler.ReadValue("ID");
        var itemJacketUnderBelt = propertiesHandler.ReadValueBool("JacketUnderBelt", false);
        var itemCanEquip = propertiesHandler.ReadValueBool("CanEquip", false);
        var itemCanScript = propertiesHandler.ReadValueBool("CanScript", false);
        var itemColorPalette = propertiesHandler.ReadValue("ColorPalette");

        // this is specific to 'DeluxeBench' exports
        itemID ??= propertiesHandler.ReadValue("ItemID");
        itemFileName ??= Path.GetFileNameWithoutExtension(iniPath);
        if (propertiesHandler.ReadValue("JacketUnderBelt") == "true") itemJacketUnderBelt = true;
        if (propertiesHandler.ReadValue("CanEquip") == "true") itemCanEquip = true;
        if (propertiesHandler.ReadValue("CanScript") == "true") itemCanScript = true;

        if (itemGameName == null || itemFileName == null || itemID == null || itemColorPalette == null)
        {
            ConsoleOutput.ShowMessage(ConsoleOutputType.Error, $"Error reading equipment folder, missing properties: GameName='{itemGameName}', FileName='{itemFileName}', ID='{itemID}', ColorPalette='{itemColorPalette}'");

            return null;
        }

        // textures
        var imageFilePaths = Directory.EnumerateFiles(folderPath, "*.png");
        var texturesByIDs = new Dictionary<int, Texture2D[]>();

        foreach (var imageFilePath in imageFilePaths)
        {
            var imageName = Path.GetFileNameWithoutExtension(imageFilePath);
            var imageNameBits = imageName.Split('_');

            int partTypeID;
            int partLocalID;
            if (!int.TryParse(imageNameBits[0], out partTypeID) || !int.TryParse(imageNameBits[1], out partLocalID)) continue;

            var texture = Textures.m_tileTextures.PremultiplyTexture(imageFilePath, game.GraphicsDevice);
            if (texture == null) continue;

            if (!texturesByIDs.ContainsKey(partTypeID))
            {
                texturesByIDs.Add(partTypeID, new Texture2D[ItemPart.TYPE.PART_RANGE]);
            }

            texturesByIDs[partTypeID][partLocalID] = texture;
        }

        // parts from textures
        foreach (var kvp in texturesByIDs)
        {
            var itemPart = new ItemPart(kvp.Value, kvp.Key, itemID);

            itemParts.Add(itemPart);
        }

        return new Item(itemParts.ToArray(), null, itemGameName, itemFileName, itemEquipmentLayer, itemID, itemJacketUnderBelt, itemCanEquip, itemCanScript, itemColorPalette);
    }

    internal static bool Load(GameSFD game)
    {
        // Items vanilla setup
        Items.m_allItems = [];
        Items.m_allFemaleItems = [];
        Items.m_allMaleItems = [];

        Items.m_slotAllItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];
        Items.m_slotFemaleItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];
        Items.m_slotMaleItems = new List<Item>[Equipment.TOTAL_INTERNAL_LAYERS];

        for (int i = 0; i < Equipment.TOTAL_INTERNAL_LAYERS; i++)
        {
            Items.m_slotAllItems[i] = [];
            Items.m_slotFemaleItems[i] = [];
            Items.m_slotMaleItems[i] = [];
        }

        // only need to check if there are duplicate IDs so
        // use a hashset instead of a dictionary, find the
        // actual item later when the exception is thrown
        var totalItemIDs = new HashSet<string>();

        var contents = SubContentHandler.GetContents()
                        .Where(content => Directory.Exists(Path.Combine(content.Directory, Constants.Paths.DATA_ITEMS)))
                        .Reverse();

        foreach (var content in contents)
        {
            var contentItemsFolderPath = Path.Combine(content.Directory, Constants.Paths.DATA_ITEMS);

            foreach (var path in Directory.EnumerateFiles(contentItemsFolderPath, "*.ini", SearchOption.AllDirectories))
            {
                if (GameSFD.Closing) return false;

                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading equipment folder: {path}");

                var item = ReadItemFromFolder(game, Path.GetDirectoryName(path), path);
                if (item == null) continue;

                // SFD vanilla exception
                if (totalItemIDs.Contains(item.ID))
                {
                    var conflictingItem = Items.GetItem(item.ID);

                    throw new Exception($"Error: Item ID collision between item '{conflictingItem}' and '{item}' while loading '{path}'");
                }

                totalItemIDs.Add(item.ID);

                Items.m_allItems.Add(item);
                Items.m_slotAllItems[item.EquipmentLayer].Add(item);

                item.PostProcess();
            }

            foreach (var path in Directory.EnumerateFiles(contentItemsFolderPath, "*.item", SearchOption.AllDirectories))
            {
                if (GameSFD.Closing) return false;

                ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading equipment file: {path}");

                var item = Content.Load<Item>(path);

                // SFD vanilla exception
                if (totalItemIDs.Contains(item.ID))
                {
                    var conflictingItem = Items.GetItem(item.ID);

                    throw new Exception($"Error: Item ID collision between item '{conflictingItem}' and '{item}' while loading '{path}'");
                }

                totalItemIDs.Add(item.ID);

                Items.m_allItems.Add(item);
                Items.m_slotAllItems[item.EquipmentLayer].Add(item);

                item.PostProcess();
            }
        }

        // Items vanilla setup end
        Items.PostProcessGenders();

        Player.HurtLevel1 = Items.GetItem("HurtLevel1");
        Player.HurtLevel2 = Items.GetItem("HurtLevel2");
        Player.HurtLevel2 ??= Player.HurtLevel1;

        Items.IsLoaded = true;
        return true;
    }
}
