using SFD;
using SFD.Code;
using SFDCT.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFDCT.Assets;

internal static class ItemLoader
{
    internal static void ThrowDuplicateItemIDException(Item existingItem, Item conflictingItem, string itemFilePath)
    {
        throw new Exception($"Error: Item ID collision between item '{existingItem}' and '{conflictingItem}' while loading '{itemFilePath}'");
    }

    internal static void Load()
    {
        Dictionary<string, Item> items = [];

        Logger.LogInfo($"LOADING [ITEMS]: Official");
        string officialItemsPath = Path.Combine(Constants.Paths.ContentPath, Constants.Paths.DATA_ITEMS);
        foreach (var filePath in Directory.GetFiles(officialItemsPath, "*.item", SearchOption.AllDirectories))
        {
            Item item = Content.Load<Item>(filePath);

            if (items.ContainsKey(item.ID))
            {
                ThrowDuplicateItemIDException(items[item.ID], item, filePath);
            }
            else
            {
                items.Add(item.ID, item);
            }
        }

        string documentsItemsPath = Path.Combine(Constants.Paths.UserDocumentsContentCustomPath, Constants.Paths.DATA_ITEMS);
        if (Directory.Exists(documentsItemsPath))
        {
            Logger.LogInfo($"LOADING [ITEMS]: Documents");
            foreach (var filePath in Directory.GetFiles(officialItemsPath, "*.item", SearchOption.AllDirectories))
            {
                Item item = Content.Load<Item>(filePath);

                if (items.ContainsKey(item.ID))
                {
                    ThrowDuplicateItemIDException(items[item.ID], item, filePath);
                }
                else
                {
                    items.Add(item.ID, item);
                }
            }
        }

        foreach (var subContentFolder in SubContentHandler.Folders)
        {
            string subContentItemsPath = SubContentHandler.GetPath(subContentFolder, Constants.Paths.DATA_ITEMS);

            if (Directory.Exists(subContentItemsPath))
            {
                Logger.LogInfo($"LOADING [ITEMS]: {subContentFolder}");

                foreach (var filePath in Directory.GetFiles(subContentItemsPath, "*.item", SearchOption.AllDirectories))
                {
                    Item item = Content.Load<Item>(filePath);

                    if (items.ContainsKey(item.ID))
                    {
                        items[item.ID] = item;
                    }
                    else
                    {
                        items.Add(item.ID, item);
                    }
                }
            }
        }

        Items.m_allItems = [];
        Items.m_allFemaleItems = [];
        Items.m_allMaleItems = [];
        Items.m_slotAllItems = new List<Item>[10];
        Items.m_slotFemaleItems = new List<Item>[10];
        Items.m_slotMaleItems = new List<Item>[10];
        for (int i = 0; i < 10; i++)
        {
            Items.m_slotAllItems[i] = [];
            Items.m_slotFemaleItems[i] = [];
            Items.m_slotMaleItems[i] = [];
        }

        foreach (var item in items.Values)
        {
            ConsoleOutput.ShowMessage(ConsoleOutputType.Loading, $"Loading equipment '{item}'");

            item.PostProcess();

            Items.m_allItems.Add(item);
            Items.m_slotAllItems[item.EquipmentLayer].Add(item);
        }

        Items.PostProcessGenders();

        Player.HurtLevel1 = Items.GetItem("HurtLevel1");
        Player.HurtLevel2 = Items.GetItem("HurtLevel2");
        Player.HurtLevel2 ??= Player.HurtLevel1;

        Items.IsLoaded = true;
    }
}
